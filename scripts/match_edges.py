import cv2
import numpy as np
from PIL import Image
import os

overlay_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Templates\Black\Xbox 360 Controller Overlay - Black.png"
press_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Theme SVG\Theme Assets\Active Presses"

ov = cv2.imread(overlay_path, cv2.IMREAD_UNCHANGED)
ov_alpha = ov[:,:,3] if ov.shape[2] == 4 else ov[:,:,0]
ov_gray = cv2.cvtColor(ov[:,:,:3], cv2.COLOR_BGR2GRAY)

results = {}

for f in sorted(os.listdir(press_dir)):
    if f.endswith('.png'):
        p = os.path.join(press_dir, f)
        btn = cv2.imread(p, cv2.IMREAD_UNCHANGED)
        if btn is None: continue
        btn_alpha = btn[:,:,3]
        btn_gray = cv2.cvtColor(btn[:,:,:3], cv2.COLOR_BGR2GRAY)
        
        # Match using edge correlation
        btn_edge = cv2.Canny(btn_gray, 50, 150)
        ov_edge = cv2.Canny(ov_gray, 50, 150)
        
        res = cv2.matchTemplate(ov_edge, btn_edge, cv2.TM_CCOEFF)
        min_v, max_v, min_loc, max_loc = cv2.minMaxLoc(res)
        results[f] = (max_loc, btn.shape[1], btn.shape[0])
        print(f"{f:32s}: loc={max_loc}, size=({btn.shape[1]}, {btn.shape[0]})")
