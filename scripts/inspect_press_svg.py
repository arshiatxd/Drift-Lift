import xml.etree.ElementTree as ET

svg_press = r'C:\Users\Parsian\Desktop\prj\312321\DriftLift\srcimg\Controller Asset Pack\Xbox Wireless Controller Images\Default Theme\Theme Assets\Xbox Series X Active Presses\Xbox Series X Active Press.svg'
tree = ET.parse(svg_press)
root = tree.getroot()

for elem in root.iter():
    tag = elem.tag.split('}')[-1]
    eid = elem.attrib.get('id', '')
    lbl = elem.attrib.get('{http://www.inkscape.org/namespaces/inkscape}label', '')
    if any(k in eid.lower() or k in lbl.lower() for k in ['btn', 'button', 'dpad', 'stick', 'bumper', 'trigger', 'home', 'view', 'menu', 'share', 'pad', 'press', 'a', 'b', 'x', 'y']):
        print(f"{tag:10s} id={eid:25s} label={lbl}")
