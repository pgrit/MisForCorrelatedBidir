import simpleimageio
import figuregen
from figuregen import util
import os
import numpy as np
import matplotlib.pyplot as plt

def colormap(img):
    cm = plt.get_cmap('RdYlBu_r')
    return cm(img[:,:,0])

def colorbar():
    cm = plt.get_cmap('RdYlBu_r')
    gradient = np.linspace(1, 0, 256)
    gradient = np.vstack((gradient, gradient))
    bar = np.swapaxes(cm(gradient), 0, 1)
    bar = np.repeat(bar, 5, axis=1)
    return bar

class FrameData:
    def __init__(self, frame_index, mindepth=3, maxdepth=3):
        self.frame_index = frame_index
        self.mindepth = mindepth
        self.maxdepth = maxdepth
        self.scene_folder = f"Results/Frame{frame_index:04}"
        self.methods = [
            ("[GGSK19]", "VarAwareRef"),
            ("Classical", "Vcm"),
            ("Ours", "PdfRatio"),
        ]
        self.method_images = [
            simpleimageio.read(os.path.join(self.scene_folder, folder, "render.exr"))
            for _, folder in self.methods
        ]
        self.reference_image = simpleimageio.read(os.path.join(self.scene_folder, "reference.exr"))

        # Compute error values
        self.errors = [
            util.image.relative_mse(m, self.reference_image, 0.001)
            for m in self.method_images
        ]

        # Gather technique pyramid images
        techniques = [
            [ f"techs-weighted{d}-merge-{k}.exr" for k in range(1, d) ]
            for d in range (3, 6)
        ]
        self.technique_images = [
            [
                [ simpleimageio.read(os.path.join(self.scene_folder, folder, "render", t)) for t in row ]
                for row in techniques
            ] for _, folder in self.methods
        ]

        raw_techniques = [
            [ f"techs-raw{d}-merge-{k}.exr" for k in range(1, d) ]
            for d in range (3, 6)
        ]
        self.raw_technique_images = [
            [
                [ simpleimageio.read(os.path.join(self.scene_folder, folder, "render", t)) for t in row ]
                for row in raw_techniques
            ] for _, folder in self.methods
        ]

        # Gather variance factor images
        varfactors = [
            [ f"variance-factors-depth-{d}-merge-{k}.exr" for k in range(2, d) ]
            for d in range(3,6)
        ]
        self.varfactor_images = [
            [ simpleimageio.read(os.path.join(self.scene_folder, "VarAwareRef", "render", t)) for t in row ]
            for row in varfactors
        ]

def compute_weight(frame: FrameData, idx: int, depth, tech):
    w = frame.technique_images[idx][depth][tech]
    r = frame.raw_technique_images[idx][depth][tech]
    result = np.zeros(w.shape)
    mask = r != 0
    result[mask] = w[mask] / r[mask]
    avg = np.mean(result[mask])
    result[r == 0] = avg
    return result, avg

def tonemap(img):
    return figuregen.JPEG(util.image.lin_to_srgb(util.image.exposure(img, 2)), quality=80)

def make_figure(frame: FrameData, title=True, add_placeholder=True):
    # Define the cropping area of the raw tech zoom-ins
    half_width = int(frame.method_images[0].shape[1] / 2)
    height = frame.method_images[0].shape[0]
    tech_crop_left = util.image.Cropbox(top=0, left=0, width=half_width, height=height, scale=1)
    tech_crop_right = util.image.Cropbox(top=0, left=half_width, width=half_width, height=height, scale=1)

    # Define the cropping areas (one half of the image each)
    third_width = int(frame.method_images[0].shape[1] / 3)
    left_crop = util.image.Cropbox(top=0, left=0, width=third_width, height=height, scale=1)
    center_crop = util.image.Cropbox(top=0, left=third_width, width=third_width, height=height, scale=1)
    right_crop = util.image.Cropbox(top=0, left=2*third_width, width=third_width, height=height, scale=1)

    renderings = figuregen.Grid(1,3)
    if title:
        renderings.set_title("top", "(c) MIS combinations")
        renderings.set_col_titles("top", [
            f'{frame.methods[0][0]}',
            f'{frame.methods[1][0]}',
            f'{frame.methods[2][0]}',
        ])
    renderings.set_col_titles("bottom", [
        f"${frame.errors[0]:.3f}$",
        f"${frame.errors[1]:.3f}$",
        f"${frame.errors[2]:.3f}$",
    ])
    left = renderings[0, 0]
    left.set_image(tonemap(left_crop.crop(frame.method_images[0])))
    center = renderings[0, 1]
    center.set_image(tonemap(center_crop.crop(frame.method_images[1])))
    right = renderings[0, 2]
    right.set_image(tonemap(right_crop.crop(frame.method_images[2])))

    weights = figuregen.Grid(1, 3)

    if title:
        weights.set_col_titles("top", [
            f"{frame.methods[0][0]}",
            f"{frame.methods[1][0]}",
            f"{frame.methods[2][0]}"
        ])
        weights.set_title("top", "(d) MIS weights")

    # Compute the MIS weight images
    left = weights[0, 0]
    w, m = compute_weight(frame, 0, 0, 0)
    w = 1 - w
    m1 = 1 - m
    left.set_image(figuregen.JPEG(colormap(left_crop.crop(w)), quality=80))

    center = weights[0, 1]
    w, m = compute_weight(frame, 1, 0, 0)
    w = 1 - w
    m2 = 1 - m
    center.set_image(figuregen.JPEG(colormap(center_crop.crop(w)), quality=80))

    right = weights[0, 2]
    w, m = compute_weight(frame, 2, 0, 0)
    w = 1 - w
    m3 = 1 - m
    right.set_image(figuregen.JPEG(colormap(right_crop.crop(w)), quality=80))

    weights.set_col_titles("bottom", [
        f"avg.: ${m1:.2f}$",
        f"${m2:.2f}$",
        f"${m3:.2f}$"
    ])

    # Add a simple color bar
    cbar = figuregen.Grid(1, 1)
    cbar[0, 0].set_image(figuregen.PNG(colorbar()))

    # Show crops of the raw technique results
    techs_grid = figuregen.Grid(1, 2)
    t2 = techs_grid[0, 0]
    t2.set_image(tonemap(tech_crop_left.crop(frame.raw_technique_images[1][0][0])))
    t1 = techs_grid[0, 1]
    t1.set_image(tonemap(tech_crop_right.crop(frame.raw_technique_images[1][0][1])))
    t2.set_frame(1, color=[0,113,188])
    t1.set_frame(1, color=[175,10,38])

    tech_error = [
        util.image.relative_mse(frame.raw_technique_images[1][0][0], frame.raw_technique_images[0][0][0]),
        util.image.relative_mse(frame.raw_technique_images[1][0][1], frame.raw_technique_images[0][0][0]),
    ]

    techs_grid.set_col_titles("bottom", [
        f"relMSE: ${tech_error[0]:.2f}$",
        f"${tech_error[1]:.2f} \\,({tech_error[1]/tech_error[0]:.2f}\\times)$",
    ])

    if title:
        techs_grid.set_col_titles("top", [
            "Merge at $\\mathbf{x}_1$", "Merge at $\\mathbf{x}_2$"
        ])
        techs_grid.set_title("top", "(b) Individual techniques")

    # placeholder for illustrations
    placeholder = figuregen.Grid(1,1)
    placeholder[0, 0].set_image(
        figuregen.PNG(util.image.lin_to_srgb(np.tile(np.array([1,1,1]), (7,2)))))
    if title:
        placeholder.set_title("top", "(a) Scene layout")

    # define the layout
    renderings.layout.set_col_titles("bottom", 2.8, offset_mm=0.5, fontsize=8)
    renderings.layout.set_padding(right=2.0, column=0.5)
    if title:
        renderings.layout.set_col_titles("top", 2.8, fontsize=8, offset_mm=0.0)
        renderings.layout.set_title("top", 2.8, fontsize=8, offset_mm=0.5)

    # align all other modules
    weights.copy_layout(renderings)
    cbar.copy_layout(renderings)
    techs_grid.copy_layout(renderings)
    placeholder.copy_layout(renderings)

    # Add extra paddings
    weights.layout.set_padding(right=0.5)
    cbar.layout.set_padding(right=4)

    if add_placeholder:
        return [placeholder, techs_grid, renderings, weights, cbar]
    else:
        return [techs_grid, renderings, weights, cbar]

if __name__ == "__main__":
    top_frame = FrameData(14)
    bot_frame = FrameData(50)
    rows = [
        make_figure(top_frame),
        make_figure(bot_frame, False)
    ]
    figuregen.figure(rows, 17.7, "Figure3.pdf", tex_packages=["{dfadobe}"])
