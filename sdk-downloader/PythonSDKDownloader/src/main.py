import os
import shutil
import Downloader
import Extractor
import ClientFiles
import Util

from Downloader import Downloader
from Extractor import Extractor
from ClientFiles import ClientFiles
from Util import Util

def main(args):
    print("SDKDownloader Started")

    sdk_root = Util.get_sdk_root()

    if len(args) > 1:
        sdk_root = args[1]
        if len(args) > 2:
            sdk_root = os.path.join(sdk_root, args[2])

    print(f"SDK Root: {sdk_root}")
    Downloader.download(sdk_root)
    Extractor.extract(sdk_root)
    ClientFiles.prepare_files(sdk_root)

    # Cleanup the Archives directory
    archives_path = Util.get_archives_path(sdk_root)
    if os.path.exists(archives_path):
        shutil.rmtree(archives_path)

    print("SDKDownloader Complete")
    return 0

if __name__ == "__main__":
    import sys
    main(sys.argv)
