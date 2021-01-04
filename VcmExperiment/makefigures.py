import numpy as np
import figuregen
from figuregen import util
from figurelayout import make_figure

paper_figures = [
    make_figure("Results/RoughGlassesIndirect",
        util.image.Cropbox(top=440, left=300, width=64*2, height=48*2, scale=10),
        util.image.Cropbox(top=500-80, left=900-0, width=64*2, height=48*2, scale=10),
        "\\textsc{Rough Glasses}", exposure=2,
        times=["84s", "84s", "400s", "84s", "810s"]),

    make_figure("Results/MinimalistWhiteRoom",
        util.image.Cropbox(top=90*2, left=5*2, width=64*2, height=48*2, scale=10),
        util.image.Cropbox(top=150*2, left=270*2, width=64*2, height=48*2, scale=10),
        "\\textsc{Minimalist Room}", exposure=2, show_method_names=False,
        times=["44s", "44s", "56s", "44s", "280s"]),

    make_figure("Results/HomeOffice",
        util.image.Cropbox(top=80*2, left=570*2, width=64*2, height=48*2, scale=10),
        util.image.Cropbox(top=170*2, left=345*2, width=64*2, height=48*2, scale=10),
        "\\textsc{Home Office} \\textsf{(a.k.a. The New Normal)}", exposure=1, show_method_names=False,
        times=["52s", "52s", "70s", "52s", "320s"])
]

additional_figures = [
    make_figure("Results/TargetPractice",
        util.image.Cropbox(top=20*2, left=370*2, width=64*2, height=48*2, scale=10),
        util.image.Cropbox(top=400*2, left=530*2, width=64*2, height=48*2, scale=10),
        "\\textsc{Target Practice}", exposure=1.5, show_method_names=True,
        times=["33s", "33s", "45s", "33s", "220s"]),

    make_figure("Results/LivingRoom",
        util.image.Cropbox(top=90*2, left=570*2, width=64*2, height=48*2, scale=10),
        util.image.Cropbox(top=300*2, left=330*2, width=64*2, height=48*2, scale=10),
        "\\textsc{Living Room}", exposure=0, show_method_names=True,
        times=["270s", "270s", "890s", "270s", "1800s"]),

    make_figure("Results/ModernHall",
        util.image.Cropbox(top=200*2, left=30*2, width=64*2, height=48*2, scale=10),
        util.image.Cropbox(top=400*2, left=530*2, width=64*2, height=48*2, scale=10),
        "\\textsc{Modern Hall}", exposure=1,
        times=["35s", "35s", "50s", "35s", "240s"]),

    make_figure("Results/ContemporaryBathroom",
        util.image.Cropbox(top=90*2, left=570*2, width=64*2, height=48*2, scale=10),
        util.image.Cropbox(top=300*2, left=190*2, width=64*2, height=48*2, scale=10),
        "\\textsc{Bathroom}",
        times=["41s", "41s", "55s", "41s", "260s"]),

    make_figure("Results/RoughGlasses",
        util.image.Cropbox(top=240*2, left=445*2, width=64*2, height=48*2, scale=10),
        util.image.Cropbox(top=260*2, left=525*2, width=64*2, height=48*2, scale=10),
        "\\textsc{Rough Glasses}",
        times=["20s", "20s", "100s", "20s", "485s"]),

    make_figure("Results/IndirectRoom",
        util.image.Cropbox(top=430, left=300, width=64*2, height=48*2, scale=10),
        util.image.Cropbox(top=747, left=1067, width=64*2, height=48*2, scale=10),
        "\\textsc{Indirect Room}", exposure=-0.5,
        times=["150s", "150s", "470s", "150s", "940s"]),

    make_figure("Results/LampCaustic",
        util.image.Cropbox(top=430-200, left=300+200, width=64*2, height=48*2, scale=10),
        util.image.Cropbox(top=747, left=1067-250, width=64*2, height=48*2, scale=10),
        "\\textsc{Lamp}", exposure=-0.5,
        times=["150s", "150s", "470s", "150s", "940s"]),

    make_figure("Results/LampCausticNoShade",
        util.image.Cropbox(top=430-200, left=300+200, width=64*2, height=48*2, scale=10),
        util.image.Cropbox(top=747, left=1067-250, width=64*2, height=48*2, scale=10),
        "\\textsc{Light Bulb}", exposure=-0.5,
        times=["150s", "150s", "470s", "150s", "940s"]),
]

width_cm = 17.7
figuregen.figure(paper_figures, width_cm, "Results/VcmFigure.pdf", tex_packages=["{dfadobe}"])
figuregen.figure(additional_figures, width_cm, "Results/VcmOther.pdf", tex_packages=["{dfadobe}"])


