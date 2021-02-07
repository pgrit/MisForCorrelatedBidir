import makefigure
import time
import threading
import figuregen
from pdf2image import convert_from_path
import numpy as np
import cv2

def make_frame(frameidx, filename):
    frame = makefigure.FrameData(frameidx)

    # make the pdf
    row = makefigure.make_figure(frame, True, False)
    figuregen.horizontal_figure(row, 17.7, filename, tex_packages=["{dfadobe}"])

    # convert to .png
    images = convert_from_path(filename, dpi=1000)
    img = np.array(images[0])
    cv2.imwrite(filename.replace(".pdf", ".png"), cv2.cvtColor(img, cv2.COLOR_RGB2BGR))

start = time.time()

# Generate the .pdf
threads = []
for i in range(1, 51):
    t = threading.Thread(target=make_frame, args=(i, f"Results/frame{i:04}.pdf"))
    t.start()
    threads.append(t)

for t in threads:
    t.join()

end = time.time()
print(f"Done generating all video frames after {time.time() - start:.2f} seconds.")
