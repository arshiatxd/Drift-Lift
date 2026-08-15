import cv2
import numpy as np
from PIL import Image
import os

xb360_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360"
base_cv = cv2.imread(os.path.join(xb360_dir, "XB360_Base.png"), cv2.IMREAD_UNCHANGED)

# Let's detect exact colored circles in ABXY region (x: 1000..1500, y: 200..600):
b, g, r, a = cv2.split(base_cv)

# Yellow Y button (high R, high G, low B)
y_pts = np.argwhere((r > 150) & (g > 150) & (b < 100))
y_cx = y_pts[:,1].mean()
y_cy = y_pts[:,0].mean()

# Red B button (high R, low G, low B)
b_pts = np.argwhere((r > 150) & (g < 80) & (b < 80))
b_cx = b_pts[:,1].mean()
b_cy = b_pts[:,0].mean()

# Blue X button (low R, high G/B)
x_pts = np.argwhere((r < 80) & (g > 120) & (b > 150))
x_cx = x_pts[:,1].mean()
x_cy = x_pts[:,0].mean()

# Green A button (low R, high G, low B)
a_pts = np.argwhere((r < 80) & (g > 150) & (b < 100))
a_cx = a_pts[:,1].mean()
a_cy = a_pts[:,0].mean()

# Guide button (x: 700..850, y: 350..550) - green X
guide_pts = np.argwhere((g > 140) & (r < 80) & (b < 80) & (np.arange(base_cv.shape[0])[:,None] < 550) & (np.arange(base_cv.shape[0])[:,None] > 350))
# Let's filter guide_pts within x in 700..850
guide_pts = [p for p in guide_pts if 700 < p[1] < 850]
guide_pts = np.array(guide_pts)
g_cx = guide_pts[:,1].mean()
g_cy = guide_pts[:,0].mean()

# Back button (triangle arrow pointing left at x ~ 580..620, y ~ 450..520)
# Start button (triangle arrow pointing right at x ~ 920..960, y ~ 450..520)

# D-pad center (cross centered at x ~ 530..570, y ~ 660..700)
# Left stick center (well centered at x ~ 340..380, y ~ 430..470)
# Right stick center (well centered at x ~ 970..1010, y ~ 660..700)

print(f"Exact Y center: ({y_cx:.1f}, {y_cy:.1f})")
print(f"Exact B center: ({b_cx:.1f}, {b_cy:.1f})")
print(f"Exact X center: ({x_cx:.1f}, {x_cy:.1f})")
print(f"Exact A center: ({a_cx:.1f}, {a_cy:.1f})")
print(f"Exact Guide center: ({g_cx:.1f}, {g_cy:.1f})")
