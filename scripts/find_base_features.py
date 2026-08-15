import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageFont
import os

base_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360\XB360_Base.png"
base = cv2.imread(base_path, cv2.IMREAD_UNCHANGED)
H, W = base.shape[:2]
print(f"Base image shape: {W}x{H}")

# Find contours of all features on XB360_Base
gray = cv2.cvtColor(base[:,:,:3], cv2.COLOR_BGR2GRAY)
edges = cv2.Canny(gray, 30, 100)

# Stick Wells:
# Left stick well is a circle at x ~ 390, y ~ 460
# Right stick well is a circle at x ~ 990, y ~ 620
# D-pad well is a circular dish at x ~ 600, y ~ 620

# Let's find stick wells by HoughCircles
circles = cv2.HoughCircles(gray, cv2.HOUGH_GRADIENT, dp=1.2, minDist=100, param1=50, param2=30, minRadius=60, maxRadius=140)

annotated = base.copy()
if circles is not None:
    circles = np.uint16(np.around(circles))
    for c in circles[0, :]:
        cx, cy, r = c[0], c[1], c[2]
        print(f"Found circle: center=({cx}, {cy}), radius={r}")
        cv2.circle(annotated, (cx, cy), r, (0, 255, 0, 255), 2)
        cv2.circle(annotated, (cx, cy), 3, (0, 0, 255, 255), 3)

cv2.imwrite(r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\annotated_base.png", annotated)
