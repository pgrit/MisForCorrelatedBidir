import simpleimageio
import cv2
import numpy as np
import os
from figuregen.util.image import lin_to_srgb

scenes = {
    "TargetPractice",
    "HomeOffice",
    "IndirectRoom",
    "RoughGlasses",
    "ModernHall",
    "LivingRoom",
    "RoughGlassesIndirect",
    "MinimalistWhiteRoom",
    "LampCaustic"
}

methods = [
    ("Bidir", "BDPT"),
    ("PathTracer", "PathTracer"),
    ("PdfRatio", "Ours"),
    ("UpperBound", "PopovEtAl"),
    ("VarAware", "GrittmannEtAl"),
    ("BidirSplit", "Balance"),
]

def png_export(img_raw, filename):
    clipped = img_raw*255
    clipped[clipped < 0] = 0
    clipped[clipped > 255] = 255
    cv2.imwrite(filename, cv2.cvtColor(clipped.astype('uint8'), cv2.COLOR_RGB2BGR))

for folder in ["Results-x10", "Results-x50", "Results-x100"]:
    for scene in scenes:
        out_folder = f"{folder}/images/" + scene
        os.makedirs(out_folder, exist_ok=True)

        # load images and write pngs
        ref = simpleimageio.read(os.path.join(folder, scene, "reference.exr"))
        png_export(lin_to_srgb(ref), os.path.join(out_folder, "reference.png"))

        for m, name in methods:
            img = simpleimageio.read(os.path.join(folder, scene, m, "render.exr"))
            png_export(lin_to_srgb(img), os.path.join(out_folder, f"{name}.png"))
