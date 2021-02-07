import simpleimageio
import figuregen
from figuregen import util
import os

scene_configs = [
    {
        "scene_folder": "Results/ModernHall",
        "scene_name": "\\textsc{Modern Hall}",
        "exposure": 1,
    },
    {
        "scene_folder": "Results/TargetPractice",
        "scene_name": "\\textsc{Target Practice}",
        "exposure": 1.5,
    },
    {
        "scene_folder": "Results/HomeOffice",
        "scene_name": "\\textsc{Home Office}",
        "exposure": 1,
    },
    {
        "scene_folder": "Results/MinimalistWhiteRoom",
        "scene_name": "\\textsc{Minimalist Room}",
        "exposure": 2,
    },
    {
        "scene_folder": "Results/IndirectRoom",
        "scene_name": "\\textsc{Indirect Room}",
        "exposure": -0.5,
    },
    {
        "scene_folder": "Results/RoughGlassesIndirect",
        "scene_name": "\\textsc{Rough Glasses}",
        "exposure": 2,
    }
]

methods = [
    ("$0.01r$", "Radius-x001"),
    ("$0.1r$", "Radius-x01"),
    ("$0.5r$", "Radius-x05"),
    ("$r$", "PdfRatioFov"),
    ("$2r$", "Radius-x2"),
    ("$10r$", "Radius-x10"),
    ("$100r$", "Radius-x100"),
    ("Balance", "Vcm"),
]

def tonemap(img, exposure):
    return figuregen.JPEG(util.image.lin_to_srgb(util.image.exposure(img, exposure)), quality=80)

def make_plot():
    rs = [1.0/100.0, 1.0/10.0, 0.5, 1, 2, 10, 100]
    data = []
    names = []

    for config in scene_configs:
        method_images = [
            simpleimageio.read(os.path.join(config["scene_folder"], folder, "render.exr"))
            for _, folder in methods
        ]
        reference_image = simpleimageio.read(os.path.join(config["scene_folder"], "reference.exr"))[:,:,:3]
        scene_err = [
            util.image.relative_mse_outlier_rejection(m, reference_image, 0.01)
            for m in method_images
        ]
        speedups = [ err / scene_err[-1] for err in scene_err ]
        speedups.pop()

        data.append([rs, speedups])
        names.append(config["scene_name"])

    plot = figuregen.PgfLinePlot(aspect_ratio=0.6, data=data)
    plot.set_padding(3.5, 5)
    plot.set_axis_label('x', "Radius scale")
    plot.set_axis_label('y', "Error")

    colors = [
        [232, 181, 88],
        [5, 142, 78],
        [94, 163, 188],
        [181, 63, 106],
        [126, 83, 172],
        [37, 85, 166],
    ]
    plot.set_colors(colors)
    plot.set_font(8, tex_package="{dfadobe}")
    plot.set_linewidth(1.5)

    plot.set_axis_properties('x', ticks=[1.0/100.0, 1.0/10.0, 0.5, 1, 2, 10])
    plot.set_axis_properties('y', ticks=[0.1, 0.5, 1.0], use_log_scale=False, range=[0.1, 1.2])

    plot.set_v_line(pos=1, color=[0,0,0], linestyle=(4,6), linewidth_pt=0.6)

    plot_module = figuregen.Grid(1,1)
    plot_module.get_element(0,0).set_image(plot)

    reference_grid = figuregen.Grid(num_rows=2, num_cols=3)
    for i in range(6):
        e = reference_grid.get_element(int(i / 3), i % 3)
        img = simpleimageio.read(os.path.join(scene_configs[i]["scene_folder"], "reference.exr"))[:,:,:3]
        tm = tonemap(img, scene_configs[i]["exposure"])
        e.set_image(tm)
        e.set_caption(scene_configs[i]["scene_name"])
        e.set_frame(3, colors[i])
    l = reference_grid.get_layout()
    l.set_caption(2.8, fontsize=8, offset_mm=0.5)
    l.set_padding(left=1)

    figuregen.figure([[plot_module, reference_grid]], 17.7, "Results/RadiusFigure.pdf",
        tex_packages=["{dfadobe}"])

make_plot()