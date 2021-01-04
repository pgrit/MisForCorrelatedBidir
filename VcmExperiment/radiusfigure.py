import pyexr
import figuregen
from figuregen import util
import os

crop_width = 90
crop_height = 80

scene_configs = [
    # {
    #     "scene_folder": "Results/ModernHall",
    #     "scene_name": "ModernHall",
    #     "crop": util.image.Cropbox(60, 400, crop_height, crop_width, 10),
    #     "exposure": 1,
    # },
    {
        "scene_folder": "Results/TargetPractice",
        "scene_name": "\\textsc{Target Practice}",
        "crop": util.image.Cropbox(40, 1000, crop_height, crop_width, 10),
        "exposure": 1.5,
    },
    # {
    #     "scene_folder": "Results/HomeOffice",
    #     "scene_name": "HomeOffice",
    #     "crop": util.image.Cropbox(160, 1140, crop_height, crop_width, 10),
    #     "exposure": 1,
    # },
    {
        "scene_folder": "Results/IndirectRoom",
        "scene_name": "\\textsc{Indirect Room}",
        "crop": util.image.Cropbox(300, 45, crop_height, crop_width, 10),
        "exposure": -0.5,
    },
    # {
    #     "scene_folder": "Results/LampCaustic",
    #     "scene_name": "LampCaustic",
    #     "crop": util.image.Cropbox(30, 30, crop_height, crop_width, 10),
    #     "exposure": -0.5,
    # },
    # {
    #     "scene_folder": "Results/LampCausticNoShade",
    #     "scene_name": "LampCausticNoShade",
    #     "crop": util.image.Cropbox(30, 30, crop_height, crop_width, 10),
    #     "exposure": 0,
    # },
    # {
    #     "scene_folder": "Results/LivingRoom",
    #     "scene_name": "LivingRoom",
    #     "crop": util.image.Cropbox(30, 30, crop_height, crop_width, 10),
    #     "exposure": 0,
    # },
    # {
    #     "scene_folder": "Results/MinimalistWhiteRoom",
    #     "scene_name": "MinimalistWhiteRoom",
    #     "crop": util.image.Cropbox(30, 30, crop_height, crop_width, 10),
    #     "exposure": 0,
    # },
    # {
    #     "scene_folder": "Results/RoughGlasses",
    #     "scene_name": "RoughGlasses",
    #     "crop": util.image.Cropbox(30, 30, crop_height, crop_width, 10),
    #     "exposure": 0,
    # },
    # {
    #     "scene_folder": "Results/RoughGlassesIndirect",
    #     "scene_name": "\\textsc{Rough Glasses}",
    #     "crop": util.image.Cropbox(460, 320, crop_height, crop_width, 10),
    #     "exposure": 2,
    # }
]

methods = [
    ("$0.01r$", "Radius-x001"),
    ("$0.1r$", "Radius-x01"),
    # ("$0.5r$", "Radius-x05"),
    ("$r$", "PdfRatioFov"),
    ("$2r$", "Radius-x2"),
    ("$10r$", "Radius-x10"),
    ("$100r$", "Radius-x100"),
    ("Balance", "Vcm"),
]

def make(config, show_title):
    method_images = [
        pyexr.read(os.path.join(config["scene_folder"], folder, "render.exr"))
        for _, folder in methods
    ]
    reference_image = pyexr.read(os.path.join(config["scene_folder"], "reference.exr"))[:,:,:3]

    # Compute error values
    errors = [
        # util.image.relative_mse(m, reference_image, 0.01)
        util.image.relative_mse_outlier_rejection(m, reference_image, 0.01)
        # util.image.smape(m, reference_image)
        for m in method_images
    ]
    error_metric_name = "relMSE"

    def error_string(index, array):
        value = f"${array[index]:.2f}$ "
        if index == len(methods)-1:
            speedup = "($1.00\\times$)"
        # elif index == 3:
        #     speedup = "\\textbf{" + f"($\\mathbf{{ {array[index]/array[len(methods)-1]:.2f}\\times }}$)" + "}"
        else:
            speedup = f"(${array[index]/array[len(methods)-1]:.2f}\\times$)"
        return "\\textsf{" + value + speedup + "}"

    def tonemap(img):
        return figuregen.PNG(util.image.lin_to_srgb(util.image.exposure(img, config["exposure"])))

    ref_grid = figuregen.Grid(1, 1)
    ref_grid.get_element(0, 0).set_image(tonemap(reference_image))
    ref_grid.get_element(0, 0).set_marker(config["crop"].get_marker_pos(), config["crop"].get_marker_size(), color=[255,255,255])
    ref_grid.set_col_titles("bottom", [config["scene_name"]])
    if show_title:
        ref_grid.get_layout().set_col_titles("top", fontsize=8, field_size_mm=2.8, offset_mm=0.25)
    ref_grid.get_layout().set_padding(right=1)
    ref_grid.get_layout().set_col_titles("bottom", fontsize=8, field_size_mm=3, offset_mm=0.25)

    crop_grid = figuregen.Grid(num_cols=len(methods), num_rows=1)
    for col in range(0, len(methods)):
        crop_grid.get_element(0, col).set_image(tonemap(config["crop"].crop(method_images[col])))
        crop_grid.get_element(0, col).set_caption(error_string(col, errors))
    if show_title:
        crop_grid.set_col_titles("top", [name for (name, _) in methods])
        crop_grid.get_layout().set_col_titles("top", fontsize=8, field_size_mm=2.8, offset_mm=0.25)
    crop_grid.get_layout().set_caption(height_mm=3, fontsize=8, offset_mm=0.25)

    return [ref_grid, crop_grid]

width_cm = 17.7
rows = []
for i in range(len(scene_configs)):
    rows.append(make(scene_configs[i], i == 0))
figuregen.figure(rows, width_cm, "Results/RadiusFigure.pdf", tex_packages=["{dfadobe}"])