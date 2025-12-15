
using System.Windows.Forms;

namespace AcmeScanner.Dialogs
{
    internal interface IScannerMessageDialog
    {
        bool IsSuccess();
        void SetStartPosition(FormStartPosition startPosition);
        void ShowDialog();
    }
}
