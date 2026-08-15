import cv2
import numpy as np
from PIL import Image
import os

xb360_dir = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\Xbox360"
base = Image.open(os.path.join(xb360_dir, "XB360_Base.png")).convert("RGBA")
W, H = base.size
print(f"XB360 Base size: {W}x{H}")

# Detect feature centers in 1545x955 space using OpenCV
base_cv = cv2.imread(os.path.join(xb360_dir, "XB360_Base.png"), cv2.IMREAD_UNCHANGED)
gray = cv2.cvtColor(base_cv[:,:,:3], cv2.COLOR_BGR2GRAY)

# Let's find centers for:
# Left Stick, Right Stick, D-Pad, Guide, Back, Start, ABXY, LB, RB, LT, RT
# Let's inspect regions:
# Y button: x: 1140..1260, y: 220..320
# X button: x: 1040..1150, y: 310..420
# B button: x: 1230..1350, y: 310..420
# A button: x: 1140..1260, y: 400..510
# Back button: x: 620..710, y: 360..450
# Start button: x: 840..930, y: 360..450
# Guide button: x: 700..850, y: 320..460
# D-pad center: x: 500..700, y: 520..720
# Left stick center: x: 260..460, y: 280..480
# Right stick center: x: 860..1060, y: 490..690

# Let's calculate exact centers:
def find_center_by_roi(roi, offset_x, offset_y):
    b, g, r = cv2.split(roi[:,:,:3])
    # brightness
    br = (r.astype(int) + g.astype(int) + b.astype(int)) / 3.0
    pts = np.argwhere(br > 80)
    if len(pts) > 0:
        cy, cx = pts.mean(axis=0)
        return offset_x + cx, offset_y + cy
    return offset_x + roi.shape[1]/2, offset_y + roi.shape[0]/2

y_cx, y_cy = find_center_by_roi(base_cv[220:320, 1150:1250], 1150, 220)
x_cx, x_cy = find_center_by_roi(base_cv[310:410, 1050:1150], 1050, 310)
b_cx, b_cy = find_center_by_roi(base_cv[310:410, 1240:1340], 1240, 310)
a_cx, a_cy = find_center_by_roi(base_cv[400:500, 1150:1250], 1150, 400)

back_cx, back_cy = find_center_by_roi(base_cv[370:430, 640:700], 640, 370)
start_cx, start_cy = find_center_by_roi(base_cv[370:430, 850:910], 850, 370)
guide_cx, guide_cy = find_center_by_roi(base_cv[320:440, 710:830], 710, 320)

dpad_cx = 598.0
dpad_cy = 618.0

ls_cx = 360.0
ls_cy = 385.0

rs_cx = 958.0
rs_cy = 618.0

print(f"Y: ({y_cx:.1f}, {y_cy:.1f}), X: ({x_cx:.1f}, {x_cy:.1f}), B: ({b_cx:.1f}, {b_cy:.1f}), A: ({a_cx:.1f}, {a_cy:.1f})")
print(f"Back: ({back_cx:.1f}, {back_cy:.1f}), Start: ({start_cx:.1f}, {start_cy:.1f}), Guide: ({guide_cx:.1f}, {guide_cy:.1f})")

# Bumper & Trigger top bounds:
# LB: x: 260..570, y: 155..295 (size 312x141)
# RB: x: 975..1260, y: 155..295 (size 285x141)
# LT: x: 280..425, y: 0..152 (size 143x152)
# RT: x: 1120..1265, y: 0..152 (size 143x152)

scale = 260.0 / 955.0
dx = (420.0 - 1545.0 * scale) / 2.0
dy = 0.0

print(f"\nScale: {scale:.6f}, dx: {dx:.2f}, dy: {dy:.2f}")

elements_360 = [
    # Face Buttons
    ("XB360_Y_Button.png", y_cx - 129/2, y_cy - 118/2, 129, 118),
    ("XB360_X_Button.png", x_cx - 126/2, x_cy - 113/2, 126, 113),
    ("XB360_B_Button.png", b_cx - 122/2, b_cy - 115/2, 122, 115),
    ("XB360_A_Button.png", a_cx - 127/2, a_cy - 106/2, 127, 106),
    
    # D-Pad
    ("XB360_D-PAD_Up.png", dpad_cx - 108/2, dpad_cy - 100, 108, 114),
    ("XB360_D-PAD_Down.png", dpad_cx - 108/2, dpad_cy - 14, 108, 114),
    ("XB360_D-PAD_Left.png", dpad_cx - 108, dpad_cy - 108/2, 134, 108),
    ("XB360_D-PAD_Right.png", dpad_cx - 26, dpad_cy - 107/2, 134, 107),
    
    # Bumpers
    ("XB360_LeftBumper_Active.png", 260, 155, 312, 141),
    ("XB360_RightBumper_Active.png", 975, 155, 285, 141),
    
    # Triggers
    ("XB360_LeftTrigger_Active.png", 280, 0, 143, 152),
    ("XB360_RightTrigger_Active.png", 1120, 0, 143, 152),
    
    # System
    ("XB360_BackButton.png", back_cx - 92/2, back_cy - 65/2, 92, 65),
    ("XB360_StartButton.png", start_cx - 92/2, start_cy - 65/2, 92, 65),
    ("XB360_GuideButton.png", guide_cx - 171/2, guide_cy - 139/2, 171, 139),
    
    # Sticks
    ("XB360_LeftStick.png", ls_cx - 180/2, ls_cy - 180/2, 180, 180),
    ("XB360_RightStick.png", rs_cx - 180/2, rs_cy - 180/2, 180, 180),
]

# Generate composite at 420x260
scaled_w = int(round(1545.0 * scale))
base_scaled = base.resize((scaled_w, 260), Image.Resampling.LANCZOS)
canvas = Image.new("RGBA", (420, 260), (0, 0, 0, 255))
canvas.paste(base_scaled, (int(round(dx)), int(round(dy))), base_scaled)

for fname, bx, by, bw, bh in elements_360:
    cx = dx + bx * scale
    cy = dy + by * scale
    cw = bw * scale
    ch = bh * scale
    p = os.path.join(xb360_dir, fname)
    if os.path.exists(p):
        btn = Image.open(p).convert("RGBA")
        btn_r = btn.resize((int(round(cw)), int(round(ch))), Image.Resampling.LANCZOS)
        canvas.alpha_composite(btn_r, (int(round(cx)), int(round(cy))))
    print(f"{fname:30s} -> Left=\"{cx:5.1f}\" Top=\"{cy:5.1f}\" Width=\"{cw:4.1f}\" Height=\"{ch:4.1f}\"")

canvas.save(r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\scripts\test_360_canvas.png")
print("Saved test_360_canvas.png")
