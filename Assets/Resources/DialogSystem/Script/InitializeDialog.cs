using UnityEngine;
using System.Collections;
using DialogSystem;

namespace DialogSystem
{
    public class InitializeDialog : MonoBehaviour
    {

        public string scriptToInitialize;

        public void StartDialog()
        {
            DialogManager.instance.Initialize(scriptToInitialize);
        }
    }

}
