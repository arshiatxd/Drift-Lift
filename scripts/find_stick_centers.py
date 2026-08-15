import cv2
import numpy as np
from PIL import Image
import os

img_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Templates\Black\Xbox 360 Controller Overlay - Black (No Thumbstick).png"
im = cv2.imread(img_path, cv2.IMREAD_UNCHANGED)
H, W = im.shape[:2]

# Detect inner circle in left stick area (x: 200..450, y: 350..600)
# Detect inner circle in right stick area (x: 850..1150, y: 550..850)

gray = cv2.cvtColor(im[:,:,:3], cv2.COLOR_BGR2GRAY)

# Left Stick ROI:
left_roi = gray[350:600, 200:450]
circles_l = cv2.HoughCircles(left_roi, cv2.HOUGH_GRADIENT, dp=1.0, minDist=30, param1=50, param2=20, minRadius=30, maxRadius=120)

# Right Stick ROI:
right_roi = gray[550:850, 850:1150]
circles_r = cv2.HoughCircles(right_roi, cv2.HOUGH_GRADIENT, dp=1.0, minDist=30, param1=50, param2=20, minRadius=30, maxRadius=120)

print(f"Image size: {W}x{H}")
if circles_l is not None:
    for c in circles_l[0, :]:
        print(f"Left Stick Circle: Center=({200 + c[0]:.1f}, {350 + c[1]:.1f}), R={c[2]:.1f}")

if circles_r is not None:
    for c in circles_r[0, :]:
        print(f"Right Stick Circle: Center=({850 + c[0]:.1f}, {550 + c[1]:.1f}), R={c[2]:.1f}")
