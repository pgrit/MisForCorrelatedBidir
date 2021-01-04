import numpy as np
import figuregen
from figuregen import util
from figurelayout import make_figure

def make(variant):
    paper_figures = [
        make_figure(f"{variant}/LivingRoom",
            util.image.Cropbox(top=40*2, left=200*2, width=64*2, height=48*2, scale=10),
            util.image.Cropbox(top=10*2, left=10*2, width=64*2, height=48*2, scale=10),
            "\\textsc{Living Room}", exposure=0, show_method_names=True,
            times=["195s", "195s", "220s", "195s"]),

        make_figure(f"{variant}/ModernHall",
            util.image.Cropbox(top=300, left=80, width=64*2, height=48*2, scale=10),
            util.image.Cropbox(top=280*2, left=280*2, width=64*2, height=48*2, scale=10),
            "\\textsc{Modern Hall}", exposure=1, show_method_names=False,
            times=["155s", "155s", "210s", "155s"])
    ]

    additional_figures = [
        make_figure(f"{variant}/MinimalistWhiteRoom",
            util.image.Cropbox(top=90*2, left=5*2, width=64*2, height=48*2, scale=10),
            util.image.Cropbox(top=150*2, left=270*2, width=64*2, height=48*2, scale=10),
            "\\textsc{Minimalist White Room}", exposure=1,
            times=["75s", "75s", "85s", "75s"]),

        make_figure(f"{variant}/TargetPractice",
            util.image.Cropbox(top=20*2, left=370*2, width=64*2, height=48*2, scale=10),
            util.image.Cropbox(top=400*2, left=530*2, width=64*2, height=48*2, scale=10),
            "\\textsc{Target Practice}", exposure=1,
            times=["260s", "260s", "280s", "260s"]),

        make_figure(f"{variant}/ContemporaryBathroom",
            util.image.Cropbox(top=90*2, left=570*2, width=64*2, height=48*2, scale=10),
            util.image.Cropbox(top=300*2, left=190*2, width=64*2, height=48*2, scale=10),
            "\\textsc{Bathroom}", exposure=1,
            times=["90s", "90s", "93s", "90s"]),

        make_figure(f"{variant}/HomeOffice",
            util.image.Cropbox(top=80*2, left=570*2, width=64*2, height=48*2, scale=10),
            util.image.Cropbox(top=170*2, left=345*2, width=64*2, height=48*2, scale=10),
            "\\textsc{Home Office}", exposure=1,
            times=["260s", "260s", "290s", "260s"]),

        make_figure(f"{variant}/RoughGlasses",
            util.image.Cropbox(top=240*2, left=445*2, width=64*2, height=48*2, scale=10),
            util.image.Cropbox(top=260*2, left=525*2, width=64*2, height=48*2, scale=10),
            "\\textsc{Rough Glasses}", exposure=0,
            times=["210s", "210s", "210s", "210s"]),

        make_figure(f"{variant}/RoughGlassesIndirect",
            util.image.Cropbox(top=240*2, left=150*2, width=64*2, height=48*2, scale=10),
            util.image.Cropbox(top=290*2, left=350*2, width=64*2, height=48*2, scale=10),
            "\\textsc{Rough Glasses}", exposure=1,
            times=["120s", "120s", "140s", "120s"]),

        make_figure(f"{variant}/IndirectRoom",
            util.image.Cropbox(top=430, left=300, width=64*2, height=48*2, scale=10),
            util.image.Cropbox(top=747, left=1067, width=64*2, height=48*2, scale=10),
            "\\textsc{Indirect Room}", exposure=0,
            times=["155s", "155s", "180s", "155s"]),
    ]

    width_cm = 17.7
    figuregen.figure(paper_figures, width_cm, f"{variant}/BidirFigure.pdf", tex_packages=["{dfadobe}"])
    figuregen.figure(additional_figures, width_cm, f"{variant}/BidirOther.pdf", tex_packages=["{dfadobe}"])

make("Results-x10")
make("Results-x50")
make("Results-x100")