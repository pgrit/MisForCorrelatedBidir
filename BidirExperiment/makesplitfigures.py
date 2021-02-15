import simpleimageio
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
    "width_mm": 20,
    "height_mm": 10,
    "fontsize": 8,
    "offset_mm": [0.0,0.0],
    "txt_padding_mm": 1,
    "bg_color": None,
    "txt_color": [255,255,255],
}

# crop = util.image.Cropbox(top=60, left=730, height=48, width=64, scale=10)
crop = util.image.Cropbox(top=300, left=50, height=48, width=64, scale=10)

i = 0
for name, factor in factors:
    bal_filename = os.path.join(factor, scene_folder, "BidirSplit", "render.exr")
    bal = simpleimageio.read(bal_filename)
    our_filename = os.path.join(factor, scene_folder, "PdfRatio", "render.exr")
    our = simpleimageio.read(our_filename)
    reference_image = simpleimageio.read(os.path.join(factor, scene_folder, "reference.exr"))

    err_bal = util.image.relative_mse_outlier_rejection(bal, reference_image, 0.01)
    err_our = util.image.relative_mse_outlier_rejection(our, reference_image, 0.01)

    combined = util.image.SplitImage([crop.crop(bal), crop.crop(our)], degree=30)
    img = tonemap(combined.get_image())

    e = grid.get_element(0, i)
    e.set_image(img)
    e.draw_lines(combined.get_start_positions(), combined.get_end_positions(),
        linewidth_pt=0.5, color=[255,255,255])

    def outline(org, outline_clr=[10,10,10], text_clr=[250,250,250]):
        res = "\\DeclareDocumentCommand{\\Outlined}{ O{black} O{white} O{0.55pt} m }{"\
                "\\contourlength{#3}"\
                "\\contour{#2}{\\textcolor{#1}{#4}}"\
            "}"
        res += "\\definecolor{FillClr}{RGB}{" + f"{text_clr[0]},{text_clr[1]},{text_clr[2]}" + "}"
        res += "\\definecolor{StrokeClr}{RGB}{" + f"{outline_clr[0]},{outline_clr[1]},{outline_clr[2]}" + "}"

        res += "\\Outlined[FillClr][StrokeClr][0.55pt]{"+ org + "}"
        return res

    e.set_label(outline("relMSE: ") + outline(f"{err_bal:.2f}") + "\n" + outline("Balance heur."),
        "top_left", **label_params)
    e.set_label(outline("Ours") + "\n" + outline("relMSE: ") + outline(f"{err_our:.2f}"),
        "bottom_right", **label_params)

    i += 1

grid.set_col_titles("bottom", ["10 shadow rays", "50 shadow rays", "100 shadow rays"])
grid.get_layout().set_col_titles("bottom", fontsize=8, field_size_mm=2.8, offset_mm=0.25)

figuregen.horizontal_figure([grid], 8.4, "SplitComparison.pdf",
    tex_packages=["{dfadobe}", "[outline]{contour}", "{color}"])