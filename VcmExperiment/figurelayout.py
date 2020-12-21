import figuregen
from figuregen import util
import pyexr
import os
from pdf2image import convert_from_path
import numpy as np
import cv2

# Load images
methods = [
    ("a) Balance", "Vcm"),
    ("b) [Jen19]", "JendersieFootprint"),
    ("c) [GGSK19]", "VarAwareMisLive"),
    ("d) \\textbf{Ours}", "PdfRatio"),
    # ("e) [GGSK19] long", "VarAwareMis"),
]

def make_figure(scene_folder, cropA, cropB, filename, scene_name, exposure=0, show_method_names=True, times=None):
    method_images = [
        pyexr.read(os.path.join(scene_folder, folder, "render.exr"))
        for _, folder in methods
    ]
    reference_image = pyexr.read(os.path.join(scene_folder, "reference.exr"))[:,:,:3]

    # Remove NaN and Inf values to not pollute the error (the right way would be to fix them ...)
    for m in method_images:
        mask = np.ma.masked_invalid(m).mask
        m[mask] = 0
    mask = np.ma.masked_invalid(reference_image).mask
    reference_image[mask] = 0

    # Compute error values
    errors = [
        # util.image.relative_mse(m, reference_image, 0.01)
        util.image.relative_mse_outlier_rejection(m, reference_image, 0.01)
        # util.image.smape(m, reference_image)
        for m in method_images
    ]
    error_metric_name = "relMSE"

    def tonemap(img):
        return util.image.lin_to_srgb(util.image.exposure(img, exposure))

    # Reference image
    ref_grid = figuregen.Grid(1, 1)
    ref_grid.get_element(0, 0).set_image(tonemap(reference_image))
    ref_grid.get_element(0, 0).set_marker(cropA.get_marker_pos(), cropA.get_marker_size(), color=[255,255,255])
    ref_grid.get_element(0, 0).set_marker(cropB.get_marker_pos(), cropB.get_marker_size(), color=[255,255,255])
    ref_grid.set_col_titles("bottom", [scene_name])

    def crop_error(img, crop):
        cropped_image = crop.crop(img)
        cropped_reference = crop.crop(reference_image)
        return util.image.relative_mse(cropped_image, cropped_reference, 0.01)
        # return util.image.smape(cropped_image, cropped_reference)

    crop_errors_A = [ crop_error(m, cropA) for m in method_images ]
    crop_errors_B = [ crop_error(m, cropB) for m in method_images ]

    def error_string(index, array, include_time=False):
        value = f"${array[index]:.2f}$ "
        if index == 0:
            speedup = "($1.00\\times$)"
        elif index == 3:
            speedup = "\\textbf{" + f"($\\mathbf{{ {array[index]/array[0]:.2f}\\times }}$)" + "}"
        else:
            speedup = f"(${array[index]/array[0]:.2f}\\times$)"

        if times is not None and include_time:
            speedup += ", " + times[index]

        return "\\textsf{" + value + speedup + "}"

    # Comparison grid
    crop_grid = figuregen.Grid(num_cols=len(methods) + 1, num_rows=2)
    for col in range(1, len(methods)+1):
        crop_grid.get_element(0, col).set_image(tonemap(cropA.crop(method_images[col-1])))
        o = 1.5 if crop_errors_A[col-1] > 10 else 0
        crop_grid.get_element(0, col).set_label(error_string(col-1, crop_errors_A), "bottom_left",
            width_mm=13.5 + o, height_mm=3, fontsize=7, offset_mm=[0.0,0.0], txt_padding_mm=0.2,
            bg_color=[40,40,40], txt_color=[255,255,255])
        # crop_grid.get_element(0, col).set_caption(error_string(col-1, crop_errors_A))

        crop_grid.get_element(1, col).set_image(tonemap(cropB.crop(method_images[col-1])))
        crop_grid.get_element(1, col).set_label(error_string(col-1, crop_errors_B), "bottom_left",
            width_mm=13.5 + o, height_mm=3, fontsize=7, offset_mm=[0.0,0.0], txt_padding_mm=0.2,
            bg_color=[40,40,40], txt_color=[255,255,255])
        # crop_grid.get_element(1, col).set_caption(error_string(col-1, crop_errors_B))

    crop_grid.get_element(0, 0).set_image(tonemap(cropA.crop(reference_image)))
    crop_grid.get_element(0, 0).set_label("\\textsf{" + f"{error_metric_name} (crop)" + "}", "bottom_left",
            width_mm=14, height_mm=3, fontsize=7, offset_mm=[0.0,0.0], txt_padding_mm=0.2,
            bg_color=[40,40,40], txt_color=[255,255,255])
    # crop_grid.get_element(0, 0).set_caption(f"{error_metric_name} (crop)")

    crop_grid.get_element(1, 0).set_image(tonemap(cropB.crop(reference_image)))
    crop_grid.get_element(1, 0).set_label("\\textsf{" + f"{error_metric_name} (crop)" + "}", "bottom_left",
            width_mm=14, height_mm=3, fontsize=7, offset_mm=[0.0,0.0], txt_padding_mm=0.2,
            bg_color=[40,40,40], txt_color=[255,255,255])
    # crop_grid.get_element(1, 0).set_caption(f"{error_metric_name} (crop)")

    # Column titles
    names = ["\\textsf{" + "Reference" + "}"]
    names.extend([ "\\textsf{" + methods[i][0] + "}" for i in range(len(methods))])

    error_strings = ["\\textsf{"
        + f"{error_metric_name}"
        + (", time" if times is not None else "")
        + "}"
    ]
    error_strings.extend([ error_string(i, errors, True) for i in range(len(methods)) ])

    if show_method_names:
        crop_grid.set_col_titles("top", names)
    crop_grid.set_col_titles("bottom", error_strings)

    # Grid layout
    crop_grid.get_layout().set_padding(column=1, row=1)
    if show_method_names:
        crop_grid.get_layout().set_col_titles("top", fontsize=8, field_size_mm=2.8, offset_mm=0.25)
    crop_grid.get_layout().set_col_titles("bottom", fontsize=8, field_size_mm=2.8, offset_mm=0.5)
    # crop_grid.get_layout().set_caption(height_mm=3, fontsize=8, offset_mm=0.25)

    # Reference layout
    ref_grid.get_layout().set_padding(right=1, bottom=0)
    if show_method_names:
        ref_grid.get_layout().set_col_titles("top", fontsize=8, field_size_mm=2.8, offset_mm=0.25)
    ref_grid.get_layout().set_col_titles("bottom", fontsize=8, field_size_mm=2.8, offset_mm=0.5)

    width_cm = 17.7
    figuregen.horizontal_figure([ref_grid, crop_grid], width_cm, filename, tex_packages=["{dfadobe}"])

def loadpdf(pdfname):
    images = convert_from_path(pdfname, dpi=1000)
    return np.array(images[0])

def convert(pdfname):
    img = loadpdf(pdfname)
    cv2.imwrite(pdfname.replace('.pdf', '.png'), cv2.cvtColor(img, cv2.COLOR_RGB2BGR))