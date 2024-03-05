
using System.Windows.Forms;

namespace AcmeScanner
{
    internal interface IScannerMessageDialog
    {
        bool IsSuccess();
        void SetStartPosition(FormStartPosition startPosition);
        void ShowDialog();
    }
}
