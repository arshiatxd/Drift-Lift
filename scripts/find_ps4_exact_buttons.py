import cv2
import numpy as np
from PIL import Image

ps4_base_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\PS4\PS4_Base.png"
base = cv2.imread(ps4_base_path, cv2.IMREAD_UNCHANGED)
H, W = base.shape[:2] # 783, 1466

# Canvas is 420x260, scale = 420/1466 = 0.28649386, dy = (260 - 783*scale)/2 = 17.84px

# Let's find the exact bounding box of the grey pill button under "SHARE" (x: 350..500, y: 240..360)
# And under "OPTIONS" (x: 950..1100, y: 240..360)

gray = cv2.cvtColor(base[:,:,:3], cv2.COLOR_BGR2GRAY)

# SHARE pill (grey button, intensity around 40..80, surrounded by black ~ 20):
roi_share = gray[260:340, 380:450]
pts_s = np.argwhere(roi_share > 40)
s_cx = 380 + pts_s[:,1].mean()
s_cy = 260 + pts_s[:,0].mean()

# OPTIONS pill:
roi_opt = gray[260:340, 1010:1080]
pts_o = np.argwhere(roi_opt > 40)
o_cx = 1010 + pts_o[:,1].mean()
o_cy = 260 + pts_o[:,0].mean()

print(f"SHARE pill center in 1466x783: ({s_cx:.1f}, {s_cy:.1f})")
print(f"OPTIONS pill center in 1466x783: ({o_cx:.1f}, {o_cy:.1f})")

# L1 bumper in 1466x783 (x: 200..380, y: 120..220)
roi_l1 = gray[120:220, 200:380]
pts_l1 = np.argwhere(roi_l1 > 40)
l1_cx = 200 + pts_l1[:,1].mean()
l1_cy = 120 + pts_l1[:,0].mean()

# R1 bumper in 1466x783 (x: 1080..1260, y: 120..220)
roi_r1 = gray[120:220, 1080:1260]
pts_r1 = np.argwhere(roi_r1 > 40)
r1_cx = 1080 + pts_r1[:,1].mean()
r1_cy = 120 + pts_r1[:,0].mean()

# L2 trigger in 1466x783 (x: 220..360, y: 0..120)
roi_l2 = gray[0:120, 220:360]
pts_l2 = np.argwhere(roi_l2 > 40)
l2_cx = 220 + pts_l2[:,1].mean()
l2_cy = 0 + pts_l2[:,0].mean()

# R2 trigger in 1466x783 (x: 1100..1240, y: 0..120)
roi_r2 = gray[0:120, 1100:1240]
pts_r2 = np.argwhere(roi_r2 > 40)
r2_cx = 1100 + pts_r2[:,1].mean()
r2_cy = 0 + pts_r2[:,0].mean()

print(f"L1 Bumper center: ({l1_cx:.1f}, {l1_cy:.1f})")
print(f"R1 Bumper center: ({r1_cx:.1f}, {r1_cy:.1f})")
print(f"L2 Trigger center: ({l2_cx:.1f}, {l2_cy:.1f})")
print(f"R2 Trigger center: ({r2_cx:.1f}, {r2_cy:.1f})")

scale = 420.0 / 1466.0
dy = (260.0 - 783.0 * scale) / 2.0
dx = 0.0

print(f"\n--- CANVAS COORDINATES ---")
print(f"SHARE:   Canvas.Left=\"{dx + s_cx * scale - 15.2/2:.1f}\" Canvas.Top=\"{dy + s_cy * scale - 24.4/2:.1f}\" Width=\"15.2\" Height=\"24.4\"")
print(f"OPTIONS: Canvas.Left=\"{dx + o_cx * scale - 15.2/2:.1f}\" Canvas.Top=\"{dy + o_cy * scale - 24.4/2:.1f}\" Width=\"15.2\" Height=\"24.4\"")
print(f"L1:      Canvas.Left=\"{dx + l1_cx * scale - 57.0/2:.1f}\" Canvas.Top=\"{dy + l1_cy * scale - 28.4/2:.1f}\" Width=\"57.0\" Height=\"28.4\"")
print(f"R1:      Canvas.Left=\"{dx + r1_cx * scale - 57.0/2:.1f}\" Canvas.Top=\"{dy + r1_cy * scale - 28.4/2:.1f}\" Width=\"57.0\" Height=\"28.4\"")
print(f"L2:      Canvas.Left=\"{dx + l2_cx * scale - 38.4/2:.1f}\" Canvas.Top=\"{dy + l2_cy * scale - 22.9/2:.1f}\" Width=\"38.4\" Height=\"22.9\"")
print(f"R2:      Canvas.Left=\"{dx + r2_cx * scale - 38.1/2:.1f}\" Canvas.Top=\"{dy + r2_cy * scale - 22.6/2:.1f}\" Width=\"38.1\" Height=\"22.6\"")
