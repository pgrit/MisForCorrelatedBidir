import figuregen
from figuregen import util
import simpleimageio
import os
import numpy as np

def outline(text, outline_clr=[10,10,10], text_clr=[250,250,250]):
    if outline_clr is None:
        res = "\\definecolor{FillClr}{RGB}{" + f"{text_clr[0]},{text_clr[1]},{text_clr[2]}" + "}"
        return res + "\\textcolor{FillClr}{" + text + "}"

    res = "\\DeclareDocumentCommand{\\Outlined}{ O{black} O{white} O{0.55pt} m }{"\
            "\\contourlength{#3}"\
            "\\contour{#2}{\\textcolor{#1}{#4}}"\
        "}"
    res += "\\definecolor{FillClr}{RGB}{" + f"{text_clr[0]},{text_clr[1]},{text_clr[2]}" + "}"
    res += "\\definecolor{StrokeClr}{RGB}{" + f"{outline_clr[0]},{outline_clr[1]},{outline_clr[2]}" + "}"

    res += "\\Outlined[FillClr][StrokeClr][0.55pt]{"+ text + "}"
    return res

def make_figure(methods, scene_folder, cropA, cropB, scene_name, exposure=0, show_method_names=True,
                times=None, outline_color=[10, 10, 10], text_color=[250, 250, 250]):
    method_images = [
        simpleimageio.read(os.path.join(scene_folder, folder, "render.exr"))
        for _, folder in methods
    ]
    reference_image = simpleimageio.read(os.path.join(scene_folder, "reference.exr"))[:,:,:3]

    # Remove NaN and Inf values to not pollute the error (the right way would be to fix them ...)
    for m in method_images:
        mask = np.ma.masked_invalid(m).mask
        m[mask] = 0
    mask = np.ma.masked_invalid(reference_image).mask
    reference_image[mask] = 0

    # Compute error values
    errors = [
        util.image.relative_mse(m, reference_image, 0.01)
        # util.image.relative_mse_outlier_rejection(m, reference_image, 0.01)
        # util.image.smape(m, reference_image)
        for m in method_images
    ]
    error_metric_name = "relMSE"

    def tonemap(img):
        return figuregen.JPEG(util.image.lin_to_srgb(util.image.exposure(img, exposure)), quality=80)

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
            speedup = "$(1.00\\times)$"
        elif index == 3:
            speedup = "\\textbf{" + f"$(\\mathbf{{ {array[index]/array[0]:.2f}\\times }})$" + "}"
        else:
            speedup = f"$({array[index]/array[0]:.2f}\\times)$"

        if times is not None and include_time:
            speedup += ", " + times[index]

        return value + speedup

    label_params = {
        "width_mm": 20,
        "height_mm": 4,
        "fontsize": 8,
        "offset_mm": [0, 0],
        "txt_padding_mm": 1,
        "bg_color": None,
        "txt_color": [255,255,255],
        "pos": "bottom_center"
    }

    def get_color(clr, i):
        if len(clr) == 2:
            return clr[i]
        else:
            return clr

    # Comparison grid
    crop_grid = figuregen.Grid(num_cols=len(methods) + 1, num_rows=2)
    for col in range(1, len(methods)+1):
        crop_grid[0, col].set_image(tonemap(cropA.crop(method_images[col-1])))
        crop_grid[0, col].set_label(outline(error_string(col-1, crop_errors_A),
            get_color(outline_color, 0), get_color(text_color, 0)),
            **label_params)

        crop_grid[1, col].set_image(tonemap(cropB.crop(method_images[col-1])))
        crop_grid[1, col].set_label(outline(error_string(col-1, crop_errors_B),
            get_color(outline_color, 1), get_color(text_color, 1)),
            **label_params)

    crop_grid[0, 0].set_image(tonemap(cropA.crop(reference_image)))
    crop_grid[0, 0].set_label(outline(f"{error_metric_name} (crop)",
        get_color(outline_color, 0), get_color(text_color, 0)),
        **label_params)

    crop_grid[1, 0].set_image(tonemap(cropB.crop(reference_image)))
    crop_grid[1, 0].set_label(outline(f"{error_metric_name} (crop)",
        get_color(outline_color, 1), get_color(text_color, 1)),
        **label_params)

    # Column titles
    names = ["Reference"]
    names.extend([ methods[i][0] for i in range(len(methods))])

    error_strings = [ f"{error_metric_name}" + (", time" if times is not None else "") ]
    error_strings.extend([ error_string(i, errors, True) for i in range(len(methods)) ])

    if show_method_names:
        crop_grid.set_col_titles("top", names)
    crop_grid.set_col_titles("bottom", error_strings)

    # Grid layout
    crop_grid.get_layout().set_padding(column=1, row=1)
    if show_method_names:
        crop_grid.get_layout().set_col_titles("top", fontsize=8, field_size_mm=2.8, offset_mm=0.25)
    crop_grid.get_layout().set_col_titles("bottom", fontsize=8, field_size_mm=2.8, offset_mm=0.5)

    # Reference layout
    ref_grid.get_layout().set_padding(right=1, bottom=0)
    if show_method_names:
        ref_grid.get_layout().set_col_titles("top", fontsize=8, field_size_mm=2.8, offset_mm=0.25)
    ref_grid.get_layout().set_col_titles("bottom", fontsize=8, field_size_mm=2.8, offset_mm=0.5)

    return [ref_grid, crop_grid]



