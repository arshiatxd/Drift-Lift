import xml.etree.ElementTree as ET
import re
import numpy as np

svg_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\DualShock 4 Controller Images\Default Theme\Theme SVG\DS4 V2 VSC SVG.svg"
tree = ET.parse(svg_path)
root = tree.getroot()

# In DS4 V2 VSC SVG.svg: viewBox = 0 0 1542.6299 824.29542
# Base PNG is 1466 x 783.
# Let's map viewBox to 1466x783:
# scale_svg_to_png_x = 1466.0 / 1542.6299 = 0.950325
# scale_svg_to_png_y = 783.0 / 824.29542 = 0.949902

# Canvas is 420x260:
# PS4_Base is 1466x783:
# Uniform scale = 420.0 / 1466.0 = 0.28649386
# Scaled height = 783.0 * scale = 224.32px
# Vertical offset dy = (260.0 - 224.32) / 2.0 = 17.84px
# Horizontal offset dx = 0.0

canvas_scale = 420.0 / 1466.0
dy = (260.0 - 783.0 * canvas_scale) / 2.0
dx = 0.0

print(f"Canvas Scale: {canvas_scale:.6f}, dx={dx:.2f}, dy={dy:.2f}")

def parse_transform(t_str):
    if not t_str:
        return np.eye(3)
    t_str = t_str.strip()
    if t_str.startswith("matrix("):
        nums = [float(x) for x in re.findall(r"[-+]?(?:\d*\.\d+|\d+)(?:[eE][-+]?\d+)?", t_str)]
        if len(nums) == 6:
            a, b, c, d, e, f = nums
            return np.array([[a, c, e], [b, d, f], [0, 0, 1]])
    elif t_str.startswith("translate("):
        nums = [float(x) for x in re.findall(r"[-+]?(?:\d*\.\d+|\d+)(?:[eE][-+]?\d+)?", t_str)]
        tx = nums[0]
        ty = nums[1] if len(nums) > 1 else 0
        return np.array([[1, 0, tx], [0, 1, ty], [0, 0, 1]])
    elif t_str.startswith("scale("):
        nums = [float(x) for x in re.findall(r"[-+]?(?:\d*\.\d+|\d+)(?:[eE][-+]?\d+)?", t_str)]
        sx = nums[0]
        sy = nums[1] if len(nums) > 1 else sx
        return np.array([[sx, 0, 0], [0, sy, 0], [0, 0, 1]])
    return np.eye(3)

# Search labeled groups in SVG
for elem in root.iter():
    lbl = elem.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', '')
    eid = elem.attrib.get('id', '')
    tag = elem.tag.split('}')[-1]
    
    if any(k == lbl for k in ['Triangle', 'Square', 'Circle', 'Cross', 'D-PAD Up', 'D-PAD Down', 'D-PAD Left', 'D-PAD Right', 'Share Button', 'Option Button', 'PS Button', 'L1', 'R1', 'Left Trigger', 'Right Trigger']):
        # Find all paths inside this group
        all_pts = []
        for p_elem in elem.iter():
            d = p_elem.attrib.get('d', '')
            if d:
                # Find numbers
                nums = [float(x) for x in re.findall(r"[-+]?(?:\d*\.\d+|\d+)(?:[eE][-+]?\d+)?", d)]
                # First absolute coordinate in d usually starts with 'm x,y' or 'M x,y'
                m_match = re.search(r"[mM]\s*([-+]?(?:\d*\.\d+|\d+))[,\s]+([-+]?(?:\d*\.\d+|\d+))", d)
                if m_match:
                    all_pts.append((float(m_match.group(1)), float(m_match.group(2))))
        if all_pts:
            pts = np.array(all_pts)
            cx, cy = pts.mean(axis=0)
            # Map svg pt to 1466x783
            png_x = cx * (1466.0 / 1542.6299)
            png_y = cy * (783.0 / 824.29542)
            # Map to canvas
            can_x = dx + png_x * canvas_scale
            can_y = dy + png_y * canvas_scale
            print(f"[{lbl:15s}]: SVG=({cx:6.1f}, {cy:6.1f}) -> PNG=({png_x:6.1f}, {png_y:6.1f}) -> Canvas: Center=({can_x:5.1f}, {can_y:5.1f})")
