using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/******************************************************************
Singleton, porte d'entrée de la gestion de la narration

Dépendances 
- lwSingleton

******************************************************************/
namespace RRNarration
{
    public class NarrationManager : lwSingleton<NarrationManager>
    {
        DatasLibrary m_datas = null;


        public void Init()
        {

        }

    }

}
