import cv2
import numpy as np
from PIL import Image

ps4_base_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\Assets\PS4\PS4_Base.png"
base = cv2.imread(ps4_base_path, cv2.IMREAD_UNCHANGED)
print("Base shape:", base.shape)

# Let's inspect the right face button region (x: 1000..1350, y: 250..550):
roi = base[250:550, 1000:1350]
# Find bright pixels in each button region:
# Triangle: y: 280..380, x: 1100..1240
# Circle: y: 360..460, x: 1190..1310
# Cross: y: 440..540, x: 1100..1240
# Square: y: 360..460, x: 1020..1140

def get_bright_center(sub_roi, off_x, off_y):
    gray = cv2.cvtColor(sub_roi[:,:,:3], cv2.COLOR_BGR2GRAY)
    pts = np.argwhere(gray > 80)
    if len(pts) > 0:
        return off_x + pts[:,1].mean(), off_y + pts[:,0].mean()
    return off_x + sub_roi.shape[1]/2, off_y + sub_roi.shape[0]/2

tri_cx, tri_cy = get_bright_center(base[280:380, 1110:1230], 1110, 280)
cir_cx, cir_cy = get_bright_center(base[360:460, 1190:1310], 1190, 360)
crs_cx, crs_cy = get_bright_center(base[440:540, 1110:1230], 1110, 440)
sqr_cx, sqr_cy = get_bright_center(base[360:460, 1020:1140], 1020, 360)

print(f"Triangle Center: ({tri_cx:.1f}, {tri_cy:.1f})")
print(f"Circle Center:   ({cir_cx:.1f}, {cir_cy:.1f})")
print(f"Cross Center:    ({crs_cx:.1f}, {crs_cy:.1f})")
print(f"Square Center:   ({sqr_cx:.1f}, {sqr_cy:.1f})")

# Let's inspect D-Pad arms:
# Up: y: 280..380, x: 260..370
# Down: y: 440..540, x: 260..370
# Left: y: 360..460, x: 190..300
# Right: y: 360..460, x: 330..440

d_up_cx, d_up_cy = get_bright_center(base[280:380, 260:370], 260, 280)
d_dn_cx, d_dn_cy = get_bright_center(base[440:540, 260:370], 260, 440)
d_lf_cx, d_lf_cy = get_bright_center(base[360:460, 190:300], 190, 360)
d_rt_cx, d_rt_cy = get_bright_center(base[360:460, 330:440], 330, 360)

print(f"D-Pad Up:    ({d_up_cx:.1f}, {d_up_cy:.1f})")
print(f"D-Pad Down:  ({d_dn_cx:.1f}, {d_dn_cy:.1f})")
print(f"D-Pad Left:  ({d_lf_cx:.1f}, {d_lf_cy:.1f})")
print(f"D-Pad Right: ({d_rt_cx:.1f}, {d_rt_cy:.1f})")

# Let's inspect Share & Options:
# Share: y: 270..340, x: 470..530
# Options: y: 270..340, x: 935..995
sh_cx, sh_cy = get_bright_center(base[270:340, 470:530], 470, 270)
opt_cx, opt_cy = get_bright_center(base[270:340, 935:995], 935, 270)

print(f"Share Center:   ({sh_cx:.1f}, {sh_cy:.1f})")
print(f"Options Center: ({opt_cx:.1f}, {opt_cy:.1f})")

# Let's inspect L1, L2, R1, R2:
# L1 bumper: y: 160..240, x: 260..450
# R1 bumper: y: 160..240, x: 1015..1205
# L2 trigger: y: 70..160, x: 290..430
# R2 trigger: y: 70..160, x: 1035..1175
l1_cx, l1_cy = get_bright_center(base[160:240, 260:450], 260, 160)
r1_cx, r1_cy = get_bright_center(base[160:240, 1015:1205], 1015, 160)
l2_cx, l2_cy = get_bright_center(base[70:160, 290:430], 290, 70)
r2_cx, r2_cy = get_bright_center(base[70:160, 1035:1175], 1035, 70)

print(f"L1 Bumper Center: ({l1_cx:.1f}, {l1_cy:.1f})")
print(f"R1 Bumper Center: ({r1_cx:.1f}, {r1_cy:.1f})")
print(f"L2 Trigger Center: ({l2_cx:.1f}, {l2_cy:.1f})")
print(f"R2 Trigger Center: ({r2_cx:.1f}, {r2_cy:.1f})")
