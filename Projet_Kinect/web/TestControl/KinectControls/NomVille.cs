using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectControls
{
    public class NomVille
    {
        private String[] listeDesVilles = { "Lille", "Paris", "Aix", "Lyon", "Nancy", "Bordeaux", "Toulouse", "Nantes", "Orléans", "Rennes" };

        private int position;

        public NomVille()
        {

        }

        public String recupererNomVille(string ville, String plusOuMoins)
        {
            int a = -1;
            if (ville == "Lille") { a = 0; }
            else if (ville == "Aix")
            {
                a = 2;
            }
            else if (ville == "Paris")
            {
                a = 1;
            }
            else if (ville == "Lyon")
            {
                a = 3;
            }
            else if (ville == "Nancy")
            {
                a = 4;
            }
            else if (ville == "Bordeaux")
            {
                a = 5;
            }
            else if (ville == "Toulouse")
            {
                a = 6;
            }
            else if (ville == "Nantes")
            {
                a = 7;
            }
            else if (ville == "Orléans")
            {
                a = 8;
            }
            else if (ville == "Rennes")
            {
                a = 9;
            }
            else 
            {
                a = 10;
            }
            
            if (plusOuMoins == "next")
            {
                a++;
            }
            else
            {
                a--;
            }
            return getNomVille(a);
        }

        public String getNomVille(int position)
        {
            this.position = position;
            if (position < 0)
            {
                this.position = listeDesVilles.Length - 1;
            }
            if (position >= listeDesVilles.Length)
            {
                this.position = 0;
            }
            return listeDesVilles[this.position];
        }
        public String getNomVilleInitiale()
        {
            this.position = 0;
            return listeDesVilles[0];
        }
        public int getPositionElement()
        {
            return position;
        }
        public int getPositionElement(String nomVille)
        {
            return positionVille(nomVille);
        }
        private int positionVille(string nomVille)
        {
            if (nomVille == "Lille")
                return 0;
            else if (nomVille == "Paris")
                return 1;
            else if (nomVille == "Aix")
                return 2;
            else if (nomVille == "Lyon")
                return 3;
            else if (nomVille == "Nancy")
                return 4;
            else if (nomVille == "Bordeaux")
                return 5;
            else if (nomVille == "Toulouse")
                return 6;
            else if (nomVille == "Nantes")
                return 7;
            else if (nomVille == "Orléans")
                return 8;
            else if (nomVille == "Rennes")
                return 9;
            else 
                return 10;
            
        }
    }
}
