import xml.etree.ElementTree as ET
import re
import numpy as np

svg_path = r"C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox 360 Controller Images\Default Theme\Theme SVG\Xbox 360 VSCView - Black.svg"
tree = ET.parse(svg_path)
root = tree.getroot()

# SVG scale to 1545x955:
# 1 mm = 3.779527559 pixels (96 DPI)
# viewBox = 0 0 408.81619 252.84766
# 408.81619 * 3.779527559 = 1545.12 px
# 252.84766 * 3.779527559 = 955.63 px

mm_to_px = 3.779527559
canvas_scale = 260.0 / 955.63
dx = (420.0 - 1545.12 * canvas_scale) / 2.0
dy = 0.0

print(f"Canvas scale: {canvas_scale:.6f}, dx={dx:.3f}")

# Function to parse transform matrix
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

# Let's traverse the tree and calculate cumulative transforms
def traverse(node, current_mat):
    t_str = node.attrib.get('transform', '')
    mat = current_mat @ parse_transform(t_str)
    lbl = node.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', '')
    eid = node.attrib.get('id', '')
    tag = node.tag.split('}')[-1]
    
    # Check for paths/ellipses/rects
    d = node.attrib.get('d', '')
    if d:
        # Extract numbers from d to get approximate coordinates
        pts = [float(x) for x in re.findall(r"[-+]?(?:\d*\.\d+|\d+)(?:[eE][-+]?\d+)?", d)]
        if len(pts) >= 2:
            # Pair pts (assuming first few points give representative coords)
            xs = pts[0::2]
            ys = pts[1::2]
            # Transform points
            t_pts = []
            for x, y in zip(xs[:30], ys[:30]):
                p = mat @ np.array([x, y, 1])
                t_pts.append((p[0] * mm_to_px, p[1] * mm_to_px))
            t_pts = np.array(t_pts)
            min_x, min_y = t_pts.min(axis=0)
            max_x, max_y = t_pts.max(axis=0)
            c_min_x = dx + min_x * canvas_scale
            c_min_y = dy + min_y * canvas_scale
            c_w = (max_x - min_x) * canvas_scale
            c_h = (max_y - min_y) * canvas_scale
            if any(k in lbl.lower() for k in ['left bumper', 'right bumper', 'left trigger', 'right trigger', 'left stick', 'right stick', 'd-pad', 'guide', 'a button', 'b button', 'x button', 'y button']):
                print(f"[{lbl:22s}] (id={eid:15s}): px_bounds=({min_x:.1f}, {min_y:.1f}, w={max_x-min_x:.1f}, h={max_y-min_y:.1f}) -> Canvas: Left=\"{c_min_x:.1f}\" Top=\"{c_min_y:.1f}\" Width=\"{c_w:.1f}\" Height=\"{c_h:.1f}\"")
                
    for child in node:
        traverse(child, mat)

traverse(root, np.eye(3))
