import json
from collections import defaultdict
import os



# Get the current working directory
current_directory = os.getcwd()

moby_json_path = os.path.join(current_directory, 'catalog', 'analog', 'server', 'moby.json')

# loading moby
with open(moby_json_path, 'r', encoding='utf-8') as file:
    moby = json.load(file)

result = defaultdict(lambda: defaultdict(lambda: {}))

def auto_patching_check():
    for product, prod_data in moby['products'].items():
        for platform, plat_data in prod_data.items():
            for signature in plat_data.get('signatures', []):
                sig_id = signature.get('signature_id')
                length_of_patching_versions = len(signature.get('patching_versions'))
                if signature.get('support_auto_patching') and length_of_patching_versions == 0:
                    if platform not in result[product]: # initialize the entry if not done already
                            result[product][platform] = {
                                sig_id: False
                            }
                    result[product][platform].update({sig_id: False})

auto_patching_check()

with open('auto_patching_check.json', 'w') as file:
     json.dump(result, file, indent=4)