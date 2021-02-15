# Add the parent directory to the PYTHONPATH so we find the common figurelayout module
import sys
sys.path.append("..")

import numpy as np
import figuregen
from figuregen import util
from figurelayout import make_figure
import concurrent.futures
import threading

crop_scale = 5
width_cm = 17.7
threads = []

methods = [
    ("(a) Balance heuristic", "BidirSplit"),
    ("(b) [PRDD15]", "UpperBound"),
    ("(c) [GGSK19]", "VarAware"),
    ("(d) \\textbf{Ours}", "PdfRatio"),
]

def make(variant):
    paper_figure_params = [
        (methods, f"{variant}/LivingRoom",
            util.image.Cropbox(top=40*2, left=200*2, width=64*2, height=48*2, scale=crop_scale),
            util.image.Cropbox(top=10*2, left=10*2, width=64*2, height=48*2, scale=crop_scale),
            "\\textsc{Living Room}", 0, True,
            ["195s", "195s", "220s", "195s"],
            [80, 50, 40], [250, 250, 250]
        ),

        (methods, f"{variant}/ModernHall",
            util.image.Cropbox(top=300, left=80, width=64*2, height=48*2, scale=crop_scale),
            util.image.Cropbox(top=280*2, left=280*2, width=64*2, height=48*2, scale=crop_scale),
            "\\textsc{Modern Hall}", 1, False,
            ["155s", "155s", "210s", "155s"],
            [[205, 197, 172], [54, 44, 34]], [[0, 0, 0], [250, 250, 250]]
        )
    ]

    additional_figure_params = [
        (methods, f"{variant}/MinimalistWhiteRoom",
            util.image.Cropbox(top=90*2, left=5*2, width=64*2, height=48*2, scale=crop_scale),
            util.image.Cropbox(top=150*2, left=270*2, width=64*2, height=48*2, scale=crop_scale),
            "\\textsc{Minimalist White Room}", 1, True,
            ["75s", "75s", "85s", "75s"]),

        (methods, f"{variant}/TargetPractice",
            util.image.Cropbox(top=20*2, left=370*2, width=64*2, height=48*2, scale=crop_scale),
            util.image.Cropbox(top=400*2, left=530*2, width=64*2, height=48*2, scale=crop_scale),
            "\\textsc{Target Practice}", 1, False,
            ["260s", "260s", "280s", "260s"]),

        (methods, f"{variant}/ContemporaryBathroom",
            util.image.Cropbox(top=90*2, left=570*2, width=64*2, height=48*2, scale=crop_scale),
            util.image.Cropbox(top=300*2, left=190*2, width=64*2, height=48*2, scale=crop_scale),
            "\\textsc{Bathroom}", 1, False,
            ["90s", "90s", "93s", "90s"]),

        (methods, f"{variant}/HomeOffice",
            util.image.Cropbox(top=80*2, left=570*2, width=64*2, height=48*2, scale=crop_scale),
            util.image.Cropbox(top=170*2, left=345*2, width=64*2, height=48*2, scale=crop_scale),
            "\\textsc{Home Office}", 1, False,
            ["260s", "260s", "290s", "260s"]),

        (methods, f"{variant}/RoughGlasses",
            util.image.Cropbox(top=240*2, left=445*2, width=64*2, height=48*2, scale=crop_scale),
            util.image.Cropbox(top=260*2, left=525*2, width=64*2, height=48*2, scale=crop_scale),
            "\\textsc{Rough Glasses}", 0, False,
            ["210s", "210s", "210s", "210s"]),

        (methods, f"{variant}/RoughGlassesIndirect",
            util.image.Cropbox(top=240*2, left=150*2, width=64*2, height=48*2, scale=crop_scale),
            util.image.Cropbox(top=290*2, left=350*2, width=64*2, height=48*2, scale=crop_scale),
            "\\textsc{Rough Glasses}", 1, False,
            ["120s", "120s", "140s", "120s"]),

        (methods, f"{variant}/IndirectRoom",
            util.image.Cropbox(top=430, left=300, width=64*2, height=48*2, scale=crop_scale),
            util.image.Cropbox(top=747, left=1067, width=64*2, height=48*2, scale=crop_scale),
            "\\textsc{Indirect Room}", 0, False,
            ["155s", "155s", "180s", "155s"]),
    ]

    paper_figures = []
    additional_figures = []
    with concurrent.futures.ThreadPoolExecutor() as executor:
        for p in paper_figure_params:
            paper_figures.append(executor.submit(make_figure, *p))
        for p in additional_figure_params:
            additional_figures.append(executor.submit(make_figure, *p))
    for i in range(len(paper_figures)):
        paper_figures[i] = paper_figures[i].result()
    for i in range(len(additional_figures)):
        additional_figures[i] = additional_figures[i].result()

    t = threading.Thread(target=figuregen.figure, args=(paper_figures, width_cm, f"{variant}/BidirFigure.pdf"),
        kwargs={"tex_packages": ["{dfadobe}", "[outline]{contour}", "{color}", "{xparse}"]})
    t.start()
    threads.append(t)

    t = threading.Thread(target=figuregen.figure, args=(additional_figures, width_cm, f"{variant}/BidirOther.pdf"),
        kwargs={"tex_packages": ["{dfadobe}", "[outline]{contour}", "{color}", "{xparse}"]})
    t.start()
    threads.append(t)

import time
start = time.time()
make("Results-x10")
make("Results-x50")
make("Results-x100")
for t in threads:
    t.join()
print(f"Generating figures done after {time.time() - start:.2f}s")