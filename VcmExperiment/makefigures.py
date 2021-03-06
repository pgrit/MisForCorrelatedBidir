# Add the parent directory to the PYTHONPATH so we find the common figurelayout module
import sys
sys.path.append("..")

import figuregen
from figuregen import util
from figurelayout import make_figure
import time
import threading
import concurrent.futures

crop_scale = 5
methods = [
    ("(a) Balance heuristic", "Vcm"),
    ("(b) [Jen19]", "JendersieFootprint"),
    ("(c) [GGSK19]", "VarAwareMisLive"),
    ("(d) \\textbf{Ours}", "PdfRatioCombined"),
]

start = time.time()

paper_figure_params = [
    (methods, "Results/RoughGlassesIndirect",
        util.image.Cropbox(top=440, left=300, width=64*2, height=48*2, scale=crop_scale),
        util.image.Cropbox(top=500-80, left=900-0, width=64*2, height=48*2, scale=crop_scale),
        "\\textsc{Rough Glasses}", 2, True,
        ["84s", "84s", "400s", "84s", "810s"], [73, 14, 3]),
    (methods, "Results/MinimalistWhiteRoom",
        util.image.Cropbox(top=90*2, left=5*2, width=64*2, height=48*2, scale=crop_scale),
        util.image.Cropbox(top=150*2, left=270*2, width=64*2, height=48*2, scale=crop_scale),
        "\\textsc{Minimalist Room}", 2, False,
        ["44s", "44s", "56s", "44s", "280s"],
        [[253,254,224], [57, 49, 30]], [[0,0,0], [250,250,250]]
    ),

    (methods, "Results/HomeOffice",
        util.image.Cropbox(top=80*2, left=570*2, width=64*2, height=48*2, scale=crop_scale),
        util.image.Cropbox(top=170*2, left=345*2, width=64*2, height=48*2, scale=crop_scale),
        "\\textsc{Home Office} (a.k.a. The New Normal)", 1, False,
        ["52s", "52s", "70s", "52s", "320s"], [39, 42, 45])
]

additional_figure_params = [
    (methods, "Results/TargetPractice",
        util.image.Cropbox(top=20*2, left=370*2, width=64*2, height=48*2, scale=crop_scale),
        util.image.Cropbox(top=400*2, left=530*2, width=64*2, height=48*2, scale=crop_scale),
        "\\textsc{Target Practice}", 1.5, True,
        ["33s", "33s", "45s", "33s", "220s"], [73,14,3]),

    (methods, "Results/LivingRoom",
        util.image.Cropbox(top=90*2, left=570*2, width=64*2, height=48*2, scale=crop_scale),
        util.image.Cropbox(top=300*2, left=330*2, width=64*2, height=48*2, scale=crop_scale),
        "\\textsc{Living Room}", 0, False,
        ["270s", "270s", "890s", "270s", "1800s"], [73,14,3]),

    (methods, "Results/ModernHall",
        util.image.Cropbox(top=200*2, left=30*2, width=64*2, height=48*2, scale=crop_scale),
        util.image.Cropbox(top=400*2, left=530*2, width=64*2, height=48*2, scale=crop_scale),
        "\\textsc{Modern Hall}", 1, False,
        ["35s", "35s", "50s", "35s", "240s"], [73,14,3]),

    (methods, "Results/RoughGlasses",
        util.image.Cropbox(top=240*2, left=445*2, width=64*2, height=48*2, scale=crop_scale),
        util.image.Cropbox(top=260*2, left=525*2, width=64*2, height=48*2, scale=crop_scale),
        "\\textsc{Rough Glasses}", 0, False,
        ["20s", "20s", "100s", "20s", "485s"], [73,14,3]),

    (methods, "Results/IndirectRoom",
        util.image.Cropbox(top=430, left=300, width=64*2, height=48*2, scale=crop_scale),
        util.image.Cropbox(top=747, left=1067, width=64*2, height=48*2, scale=crop_scale),
        "\\textsc{Indirect Room}", -0.5, False,
        ["150s", "150s", "470s", "150s", "940s"], [73,14,3]),

    (methods, "Results/LampCaustic",
        util.image.Cropbox(top=430-200, left=300+200, width=64*2, height=48*2, scale=crop_scale),
        util.image.Cropbox(top=747, left=1067-250, width=64*2, height=48*2, scale=crop_scale),
        "\\textsc{Lamp}", -0.5, False,
        ["150s", "150s", "470s", "150s", "940s"], [73,14,3]),

    (methods, "Results/LampCausticNoShade",
        util.image.Cropbox(top=430-200, left=300+200, width=64*2, height=48*2, scale=crop_scale),
        util.image.Cropbox(top=747, left=1067-250, width=64*2, height=48*2, scale=crop_scale),
        "\\textsc{Light Bulb}", -0.5, False,
        ["150s", "150s", "470s", "150s", "940s"], [73,14,3]),
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

print(f"Processing data took {time.time() - start:.2f}s")
start = time.time()

width_cm = 17.7
t1 = threading.Thread(target = figuregen.figure, args=(paper_figures, width_cm, "Results/VcmFigure.pdf"),
    kwargs={"tex_packages": ["{dfadobe}", "[outline]{contour}", "{color}", "{xparse}"]})
t2 = threading.Thread(target = figuregen.figure, args=(additional_figures, width_cm, "Results/VcmOther.pdf"),
    kwargs={"tex_packages": ["{dfadobe}", "[outline]{contour}", "{color}", "{xparse}"]})
t1.start()
t2.start()
t1.join()
t2.join()

print(f"Generating figures took {time.time() - start:.2f}s")

