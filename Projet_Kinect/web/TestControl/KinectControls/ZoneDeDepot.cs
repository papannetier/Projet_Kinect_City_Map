using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace KinectControls
{
    public class ZoneDeDepot
    {
        private List<HoverButton> listZoneDepot;
        private int nombreErreur = 0;
        private String response = "";
        private int nbreLabelplace = -1;

        public ZoneDeDepot(List<HoverButton> list)
        {
            this.listZoneDepot = list;
        }

        public void verifierCorrespondanceZoneLabel()
        {
            if (labelTousMis())
            {
                nombreErreur = 0;
                foreach (HoverButton zoneText in listZoneDepot)
                {                    
                    String convertText = "zone" + zoneText.Text;
                    if (convertText == zoneText.Name)
                    {
                        Console.WriteLine("bonne réponse: " + zoneText.Name);
                        //TODO mettre la zoneText en vert.
                        zoneText.TextColor = Brushes.Green;
                    }
                    else
                    {
                        nombreErreur++;
                        Console.WriteLine("mauvaise réponse: " + zoneText.Name);
                        //TODO mettre la zoneText en rouge + mettre en dessous la réponse.
                        zoneText.TextColor = Brushes.Red;
                    }                    
                }
                response = "CorrectionFaite";
            }
            else
            {
                response = "Pour avoir une correction, veuillez remplir l'ensemble des zones!";
            }
        }
        public String afficheDetailCorrection()
        {
            return "Vous avez placé " + (listZoneDepot.Count - nombreErreur) + "/" + listZoneDepot.Count + " correctement";
        }
        private Boolean labelTousMis()
        {
            int nbreIsPasNull = 0;
            foreach (HoverButton zoneText in listZoneDepot)
            {
                if (zoneText.Text != "")
                {
                    nbreIsPasNull++;
                }
            }
            if (nbreIsPasNull == listZoneDepot.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public String reponse()
        {
            return response;
        }
        public void verifierDoublonsOnMap(HoverButton zone)//verifie et supprime les doublons de la map
        {
            nbreLabelplace = 0;
            foreach (HoverButton zoneText in listZoneDepot)
            {
                if ((zoneText.Name != zone.Name) && (zoneText.Text == zone.Text))
                {
                    zoneText.Text = "";
                    zoneText.Opacity = 0;
                }
                if (zoneText.Text != "")
                {
                    nbreLabelplace++;
                    TextRenvoyePourLabelPlace();
                }
            }
        }
        private void TextRenvoyePourLabelPlace()
        {
            if (nbreLabelplace == 0)
            {
                response = "Veuillez placer toutes les villes sur la carte";
            }
            else if (nbreLabelplace == 1)
            {
                response = "Vous avez placé une ville,"+"\n"+"plus que neuf à placer.";
            }
            else if (nbreLabelplace == 2)
            {
                response = "Vous avez placé deux villes,"+"\n"+"plus que huit à placer.";
            }
            else if (nbreLabelplace == 3)
            {
                response = "Vous avez placé trois villes," + "\n" + "plus que sept à placer.";
            }
            else if (nbreLabelplace == 4)
            {
                response = "Vous avez placé quatre villes," + "\n" + "plus que six à placer.";
            }
            else if (nbreLabelplace == 5)
            {
                response = "Vous avez placé cinq villes," + "\n" + "plus que cinq à placer.";
            }
            else if (nbreLabelplace == 6)
            {
                response = "Vous avez placé six villes," + "\n" + "plus que quatre à placer.";
            }
            else if (nbreLabelplace == 7)
            {
                response = "Vous avez placé sept villes," + "\n" + "plus que trois à placer.";
            }
            else if (nbreLabelplace == 8)
            {
                response = "Vous avez placé huit villes," + "\n" + "plus que deux à placer.";
            }
            else if (nbreLabelplace == 9)
            {
                response = "Vous avez placé neuf villes," + "\n" + "plus qu'une à placer avant d'avoir la correction.";
            }
            else if (nbreLabelplace == 10)
            {
                response = "Maintenant que les villes sont placées. Vous pouvez demander la correction";
            }
        }
        public void initialise()
        {
            foreach (HoverButton zoneText in listZoneDepot)
            {
                zoneText.TextColor = Brushes.White;
                zoneText.Text = "";
            }
        }
    }
}