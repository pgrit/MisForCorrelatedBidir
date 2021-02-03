import pyexr
import figuregen
from figuregen import util
import os

factors = [
    ("10", "Results-x10"),
    ("50", "Results-x50"),
    ("100", "Results-x100"),
]

scene_folder = "IndirectRoom"

def tonemap(img):
    return figuregen.JPEG(util.image.lin_to_srgb(img), quality=80)

grid = figuregen.Grid(num_cols=3, num_rows=1)

label_params = {
    "width_mm": 13.5,
    "height_mm": 10,
    "fontsize": 7,
    "offset_mm": [0.0,0.0],
    "txt_padding_mm": 0.2,
    "bg_color": None,
    "txt_color": [255,255,255],
}

# crop = util.image.Cropbox(top=60, left=730, height=48, width=64, scale=10)
crop = util.image.Cropbox(top=300, left=50, height=48, width=64, scale=10)

i = 0
for name, factor in factors:
    bal_filename = os.path.join(factor, scene_folder, "BidirSplit", "render.exr")
    bal = pyexr.read(bal_filename)
    our_filename = os.path.join(factor, scene_folder, "PdfRatio", "render.exr")
    our = pyexr.read(our_filename)
    reference_image = pyexr.read(os.path.join(factor, scene_folder, "reference.exr"))

    err_bal = util.image.relative_mse_outlier_rejection(bal, reference_image, 0.01)
    err_our = util.image.relative_mse_outlier_rejection(our, reference_image, 0.01)

    combined = util.image.SplitImage([crop.crop(bal), crop.crop(our)], degree=30)
    img = tonemap(combined.get_image())

    e = grid.get_element(0, i)
    e.set_image(img)
    e.draw_lines(combined.get_start_positions(), combined.get_end_positions(),
        linewidth_pt=0.5, color=[255,255,255])

    contour = "\\contourlength{0.5pt} \\contournumber{20}"
    def make_contour(org):
        return "\\contour{black}{" + org + "}"
    e.set_label(contour + make_contour("relMSE: ") + make_contour(f"{err_bal:.2f}") + "\n" + make_contour("Balance"),
        "top_left", **label_params)
    e.set_label(contour + make_contour("Ours") + "\n" + make_contour("relMSE: ") + make_contour(f"{err_our:.2f}"),
        "bottom_right", **label_params)

    i += 1

grid.set_col_titles("bottom", ["10 shadow rays", "50 shadow rays", "100 shadow rays"])
grid.get_layout().set_col_titles("bottom", fontsize=8, field_size_mm=2.8, offset_mm=0.25)

figuregen.horizontal_figure([grid], 8.4, "SplitComparison.pdf",
    tex_packages=["{dfadobe}", "{contour}", "{color}"])