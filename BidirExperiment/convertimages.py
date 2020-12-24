import pyexr
import cv2
import numpy as np
import os
from figuregen.util.image import lin_to_srgb

scenes = {
    "BananaRange",
    "ContemporaryBathroom",
    "HomeOffice",
    "IndirectRoom",
    "RoughGlasses",
    "ModernHall",
    "LivingRoom",
    "RoughGlassesIndirect",
    "MinimalistWhiteRoom",
}

methods = [
    ("Bidir", "BDPT"),
    ("PathTracer", "PathTracer"),
    ("PdfRatioVcm", "Ours"),
    ("UpperBound", "PopovEtAl"),
    ("VarAware", "GrittmannEtAl"),
    ("BidirSplit", "Balance"),
]

def png_export(img_raw, filename):
    clipped = img_raw*255
    clipped[clipped < 0] = 0
    clipped[clipped > 255] = 255
    cv2.imwrite(filename, cv2.cvtColor(clipped.astype('uint8'), cv2.COLOR_RGB2BGR))

for scene in scenes:
    out_folder = "results/images/" + scene
    os.makedirs(out_folder, exist_ok=True)

    # load images and write pngs
    ref = pyexr.read(os.path.join("results", scene, "reference.exr"))
    png_export(lin_to_srgb(ref), os.path.join(out_folder, "reference.png"))

    for m, name in methods:
        img = pyexr.read(os.path.join("results", scene, m, "render.exr"))
        png_export(lin_to_srgb(img), os.path.join(out_folder, f"{name}.png"))
