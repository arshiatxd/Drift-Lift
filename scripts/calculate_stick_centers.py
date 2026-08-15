import cv2
import numpy as np
from PIL import Image

img_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Templates\Black\Xbox 360 Controller Overlay - Black (No Thumbstick).png"
im = cv2.imread(img_path, cv2.IMREAD_UNCHANGED)
gray = cv2.cvtColor(im[:,:,:3], cv2.COLOR_BGR2GRAY)

# In the left well (x: 200..450, y: 350..600):
# The inner circular base is shaded (gray value around 50..90)
# Let's find the inner circle centroid:
roi_l = gray[350:600, 200:450]
pts_l = np.argwhere((roi_l > 40) & (roi_l < 90))
cx_l = 200 + pts_l[:,1].mean()
cy_l = 350 + pts_l[:,0].mean()

# In the right well (x: 850..1150, y: 550..850):
roi_r = gray[550:850, 850:1150]
pts_r = np.argwhere((roi_r > 40) & (roi_r < 90))
cx_r = 850 + pts_r[:,1].mean()
cy_r = 550 + pts_r[:,0].mean()

print(f"Left Stick Well Inner Center: ({cx_l:.1f}, {cy_l:.1f})")
print(f"Right Stick Well Inner Center: ({cx_r:.1f}, {cy_r:.1f})")

scale = 260.0 / 955.0
dx = (420.0 - 1545.0 * scale) / 2.0
dy = 0.0

# Stick cap size is 49.0 x 49.0
# Center to Top-Left in canvas:
l_left = dx + (cx_l * scale) - (49.0 / 2.0)
l_top = dy + (cy_l * scale) - (49.0 / 2.0)

r_left = dx + (cx_r * scale) - (49.0 / 2.0)
r_top = dy + (cy_r * scale) - (49.0 / 2.0)

print(f"Left Stick Grid: Canvas.Left=\"{l_left:.1f}\" Canvas.Top=\"{l_top:.1f}\" Width=\"49.0\" Height=\"49.0\"")
print(f"Right Stick Grid: Canvas.Left=\"{r_left:.1f}\" Canvas.Top=\"{r_top:.1f}\" Width=\"49.0\" Height=\"49.0\"")
