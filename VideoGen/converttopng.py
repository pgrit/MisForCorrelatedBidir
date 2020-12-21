from pdf2image import convert_from_path
import numpy as np
import cv2

# methods = ['varaware', 'ours']
# cases = ['comparison', 'weights']

methods = ['combined']
cases = ['weights']

for i in range(1, 51):
    for m in methods:
        for c in cases:
            images = convert_from_path(f'./results/{c}/{m}/frame{i:04}.pdf', dpi=1000)
            img = np.array(images[0])
            cv2.imwrite(f"./results/{c}/{m}/frame{i:04}.png",
                        cv2.cvtColor(img, cv2.COLOR_RGB2BGR))