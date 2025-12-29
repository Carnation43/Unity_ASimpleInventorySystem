using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test_MailPanel : MonoBehaviour
{
    [SerializeField] private Test_RedDotView mainMail;
    [SerializeField] private Test_RedDotView subMail;
    [SerializeField] private Test_RedDotView subSpecialMail;
    [SerializeField] private Test_RedDotView mail;
    [SerializeField] private Test_RedDotView specialMail;

    private string _mailPath;

    private string _specialMailPath;

    private void Start()
    {
        _mailPath = Test_RedDotSystem.Test_RedDotPaths.Mail;
        _specialMailPath = Test_RedDotSystem.Test_RedDotPaths.SpecialMail;

        mainMail.SetPath(Test_RedDotSystem.Test_RedDotPaths.MainMail);
        subMail.SetPath(Test_RedDotSystem.Test_RedDotPaths.SubMail);
        subSpecialMail.SetPath(Test_RedDotSystem.Test_RedDotPaths.SubSpecialMail);
        mail.SetPath(Test_RedDotSystem.Test_RedDotPaths.Mail);
        specialMail.SetPath(Test_RedDotSystem.Test_RedDotPaths.SpecialMail);
    }

    [ContextMenu("DEBUG: Acquire a mail")]
    public void Debug_AcquireMail()
    {
        var node = Test_RedDotSystem.Instance.GetNode(_mailPath, true);
        node.SetCount(node.Count + 1);
    }

    [ContextMenu("DEBUG: Acuqire a specialMail")]
    public void Debug_AcquireQuest()
    {
        var node = Test_RedDotSystem.Instance.GetNode(_specialMailPath, true);
        node.SetCount(node.Count + 1);
    }
}
