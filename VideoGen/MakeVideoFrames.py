import pyexr
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
    def __init__(self, frame_index):
        self.frame_index = frame_index
        self.scene_folder = f"Results/Frame{frame_index:04}"
        self.methods = [
            ("VarAware", "VarAwareRef"),
            ("Balance", "Vcm"),
            ("Ours", "PdfRatio"),
        ]
        self.method_images = [
            pyexr.read(os.path.join(self.scene_folder, folder, "render.exr"))
            for _, folder in self.methods
        ]
        self.reference_image = pyexr.read(os.path.join(self.scene_folder, "reference.exr"))

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
                [ pyexr.read(os.path.join(self.scene_folder, folder, "render", t)) for t in row ]
                for row in techniques
            ] for _, folder in self.methods
        ]

        raw_techniques = [
            [ f"techs-raw{d}-merge-{k}.exr" for k in range(1, d) ]
            for d in range (3, 6)
        ]
        self.raw_technique_images = [
            [
                [ pyexr.read(os.path.join(self.scene_folder, folder, "render", t)) for t in row ]
                for row in raw_techniques
            ] for _, folder in self.methods
        ]

        # Gather variance factor images
        varfactors = [
            [ f"variance-factors-depth-{d}-merge-{k}.exr" for k in range(2, d) ]
            for d in range(3,6)
        ]
        self.varfactor_images = [
            [ pyexr.read(os.path.join(self.scene_folder, "VarAwareRef", "render", t)) for t in row ]
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

def make_figure(frame: FrameData, filename, title=True):
    # Define the cropping area of the raw tech zoom-ins
    half_width = int(frame.method_images[0].shape[1] / 2)
    height = frame.method_images[0].shape[0]
    tech_crop_left = util.image.Cropbox(top=0, left=0, width=half_width, height=height, scale=2)
    tech_crop_right = util.image.Cropbox(top=0, left=half_width, width=half_width, height=height, scale=2)

    # Define the cropping areas (one half of the image each)
    third_width = int(frame.method_images[0].shape[1] / 3)
    left_crop = util.image.Cropbox(top=0, left=0, width=third_width, height=height, scale=2)
    center_crop = util.image.Cropbox(top=0, left=third_width, width=third_width, height=height, scale=2)
    right_crop = util.image.Cropbox(top=0, left=2*third_width, width=third_width, height=height, scale=2)

    renderings = figuregen.Grid(1,3)
    if title:
        renderings.set_title("top", "\\textsf{c) Renderings}")
        renderings.set_col_titles("top", [
            f'\\textsf{{{frame.methods[0][0]}}}',
            f'\\textsf{{{frame.methods[1][0]}}}',
            f'\\textsf{{{frame.methods[2][0]}}}',
        ])
    renderings.set_col_titles("bottom", [
        f"\\textsf{{${frame.errors[0]:.3f}$}}",
        f"\\textsf{{${frame.errors[1]:.3f}$}}",
        f"\\textsf{{${frame.errors[2]:.3f}$}}",
    ])
    left = renderings.get_element(0, 0)
    left.set_image(util.image.lin_to_srgb(left_crop.crop(frame.method_images[0])))
    center = renderings.get_element(0, 1)
    center.set_image(util.image.lin_to_srgb(center_crop.crop(frame.method_images[1])))
    right = renderings.get_element(0, 2)
    right.set_image(util.image.lin_to_srgb(right_crop.crop(frame.method_images[2])))

    weights = figuregen.Grid(1, 3)

    if title:
        weights.set_col_titles("top", [
            f"\\textsf{{{frame.methods[0][0]}}}",
            f"\\textsf{{{frame.methods[1][0]}}}",
            f"\\textsf{{{frame.methods[2][0]}}}"
        ])
        weights.set_title("top", r"\textsf{d) MIS weights}")

    # Compare the first merge
    left = weights.get_element(0, 0)
    w, m = compute_weight(frame, 0, 0, 0)
    w = 1 - w
    m1 = 1 - m
    left.set_image(colormap(left_crop.crop(w)))

    center = weights.get_element(0, 1)
    w, m = compute_weight(frame, 1, 0, 0)
    w = 1 - w
    m2 = 1 - m
    center.set_image(colormap(center_crop.crop(w)))

    right = weights.get_element(0, 2)
    w, m = compute_weight(frame, 2, 0, 0)
    w = 1 - w
    m3 = 1 - m
    right.set_image(colormap(right_crop.crop(w)))

    weights.set_col_titles("bottom", [
        f"\\textsf{{avg.: ${m1:.2f}$}}",
        f"\\textsf{{${m2:.2f}$}}",
        f"\\textsf{{${m3:.2f}$}}"
    ])

    cbar = figuregen.Grid(1, 1)
    cbar.get_element(0, 0).set_image(colorbar())

    # Show crops of the raw technique results
    techs_grid = figuregen.Grid(1, 2)

    t2 = techs_grid.get_element(0, 0)
    def tonemap(img):
        return util.image.lin_to_srgb(util.image.exposure(img, 2))
    t2.set_image(tonemap(tech_crop_left.crop(frame.raw_technique_images[1][0][0])))

    t1 = techs_grid.get_element(0, 1)
    t1.set_image(tonemap(tech_crop_right.crop(frame.raw_technique_images[1][0][1])))

    t2.set_frame(1, color=[0,113,188])
    t1.set_frame(1, color=[175,10,38])

    tech_error = [
        util.image.relative_mse(frame.raw_technique_images[1][0][0], frame.raw_technique_images[0][0][0]),
        util.image.relative_mse(frame.raw_technique_images[1][0][1], frame.raw_technique_images[0][0][0]),
    ]

    techs_grid.set_col_titles("bottom", [
        f"\\textsf{{relMSE: ${tech_error[0]:.2f}$}}",
        f"\\textsf{{${tech_error[1]:.2f} \\,({tech_error[1]/tech_error[0]:.2f}\\times)$}}",
    ])

    if title:
        techs_grid.set_col_titles("top", [
            r"\textsf{Merge at $\mathbf{x}_1$}", r"\textsf{Merge at $\mathbf{x}_2$}"
        ])
        techs_grid.set_title("top", "\\textsf{b) Individual techniques}")

    # placeholder for illustrations
    placeholder = figuregen.Grid(1,1)
    placeholder.get_element(0, 0).set_image(util.image.lin_to_srgb(np.tile(np.array([1,1,1]), (7,2))))
    if title:
        placeholder.set_title("top", "\\textsf{a) Layout}")

    # Define and align the layout
    renderings.get_layout().set_col_titles("bottom", 2.8, offset_mm=0.5, fontsize=8)
    weights.get_layout().set_col_titles("bottom", 2.8, offset_mm=0.5, fontsize=8)
    cbar.get_layout().set_col_titles("bottom", 2.8, offset_mm=0.5, fontsize=8)
    techs_grid.get_layout().set_col_titles("bottom", 2.8, offset_mm=0.5, fontsize=8)

    if title:
        renderings.get_layout().set_col_titles("top", 2.8, fontsize=8, offset_mm=0.0)
        weights.get_layout().set_col_titles("top", 2.8, fontsize=8, offset_mm=0.0)
        cbar.get_layout().set_col_titles("top", 2.8, fontsize=8, offset_mm=0.0)
        techs_grid.get_layout().set_col_titles("top", 2.8, fontsize=8, offset_mm=0.0)
        placeholder.get_layout().set_col_titles("top", 2.8, fontsize=8, offset_mm=0.0)

        renderings.get_layout().set_title("top", 2.8, fontsize=8, offset_mm=0.5)
        weights.get_layout().set_title("top", 2.8, fontsize=8, offset_mm=0.5)
        cbar.get_layout().set_title("top", 2.8, fontsize=8, offset_mm=0.5)
        techs_grid.get_layout().set_title("top", 2.8, fontsize=8, offset_mm=0.5)
        placeholder.get_layout().set_title("top", 2.8, fontsize=8, offset_mm=0.5)

    weights.get_layout().set_padding(right=0.5, column=0.5)
    renderings.get_layout().set_padding(right=2.0, column=0.5)
    techs_grid.get_layout().set_padding(right=2.0, column=0.5)
    placeholder.get_layout().set_padding(right=2.0, column=0.5)
    cbar.get_layout().set_padding(right=4, column=0.5)

    figuregen.horizontal_figure([techs_grid, renderings, weights, cbar], 17.7, filename, tex_packages=["{dfadobe}"])

import time
start = time.time()

import threading
threads = []
for i in range(1, 51):
    frame = FrameData(i)
    t = threading.Thread(target=make_figure, args=(frame, f"Results/weights/combined/frame{i:04}.pdf"))
    t.start()
    threads.append(t)

for t in threads:
    t.join()

end = time.time()
print (end - start)

# Convert the frames to png
from pdf2image import convert_from_path
import numpy as np
import cv2

methods = ['combined']
cases = ['weights']

for i in range(1, 51):
    for m in methods:
        for c in cases:
            images = convert_from_path(f'./results/{c}/{m}/frame{i:04}.pdf', dpi=1000)
            img = np.array(images[0])
            cv2.imwrite(f"./results/{c}/{m}/frame{i:04}.png",
                        cv2.cvtColor(img, cv2.COLOR_RGB2BGR))