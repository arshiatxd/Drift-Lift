import xml.etree.ElementTree as ET

svg_path = r'C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox Wireless Controller Images\Default Theme\Theme SVG\Xbox Series X Color\Xbox Series X Controller VSCView Black.svg'
tree = ET.parse(svg_path)
root = tree.getroot()

controller_g = None
for g in root.iter('{http://www.w3.org/2000/svg}g'):
    if g.attrib.get('id') == 'g21715':
        controller_g = g
        break

if controller_g is not None:
    for group in controller_g:
        label = group.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', group.attrib.get('id', ''))
        gid = group.attrib.get('id', '')
        print(f"=== {label} (id={gid}) ===")
        for elem in group:
            sub_lbl = elem.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', elem.attrib.get('id', ''))
            tag = elem.tag.split('}')[-1]
            eid = elem.attrib.get('id', '')
            print(f"   {tag:6s} id={eid:15s} label={sub_lbl}")
