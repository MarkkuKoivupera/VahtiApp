﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;

namespace VahtiApp
{
    internal class Tarjous : IComparable<Tarjous>, IEquatable<Tarjous>
    {
        static int nro = 0;
        static Tarjous clStaticMuisti;
        

        public const int LuokkaVersion = 20200610;
        public string strKunta { get; set; }
        public string strTunnus { get; set; }
        public string strAlkuperainenLinkki { get; set; }
        public string strTajousDocLinkki { get; set; }
        public string strTarjousDirLinkki { get; set; }
        private string strPrivPyynto { get; set; }
        public string strPyynto
        {
            get
            {
                return strPrivPyynto.Replace("\r", " ").Replace("\n", " ");
            }
            set
            {

                strPrivPyynto = value.Replace("\r", " ").Replace("\n", " ");

            }
        }
        public string strKuvaus { get; set; }
        public string strMaaraAika
        {
            get { return dtMaaraAika.ToString("yyyyMMdd_HHmm"); }
            //set { dtAika = DateTime.ParseExact(value, "dd.MM.yyyy HH:mm:ss", null); } 
            set
            {
                if (value.Contains("_"))
                {
                    dtMaaraAika = DateTime.ParseExact(value, "yyyyMMdd_HHmm", new CultureInfo("fi-FI"));
                }
                else
                if (value.Contains("."))
                {
                    if (!value.Contains("mää"))
                        dtMaaraAika = Convert.ToDateTime(value, new CultureInfo("fi-FI"));
                    else
                    {
                        dtMaaraAika = new DateTime(3000, 12, 31, 23, 59, 59);
                        strFiltered = "true";
                    }
                }
            }
        }

        public string strDataBase { get; internal set; }
        public string strKommentti { get; internal set; }
        public string strJulkaistu
        {
            get { return dtJulkaistu.ToString("yyyyMMdd_HHmm"); }
            //set { dtAika = DateTime.ParseExact(value, "dd.MM.yyyy HH:mm:ss", null); } 
            set
            {
                if (value.Contains("_"))
                {
                    dtJulkaistu = DateTime.ParseExact(value, "yyyyMMdd_HHmm", new CultureInfo("fi-FI"));
                }
                else
                if (value.Contains("."))
                {
                    if (!value.Contains("jul"))
                        dtJulkaistu = Convert.ToDateTime(value, new CultureInfo("fi-FI"));
                    else
                        dtJulkaistu = DateTime.Now;
                }

            }
        }
        public string strFiltered
        {
            get { return bFiltered.ToString(); }
            internal set { 
                bFiltered = value.ToLower().Equals("true"); 
            }
        }
        public string strIlmoitusTyyppi { get; internal set; }
        public string strKuvaushaettu
        {
            get { return bkuvausHaettu.ToString(); }
            internal set { bkuvausHaettu = value.ToLower().Equals("true"); }
        }
        public string strSuodatettu { get; internal set; }
        public string strVaihtoehtoLinkki { get; internal set; }
        public string strKiinnostus
        {
            get { return iKiinnostus.ToString(); }
            internal set
            {
                iKiinnostus = Convert.ToInt32(value);
            }
        }
        public int iKiinnostus { get; set; }

        private DateTime dtMaaraAika;
        private DateTime dtJulkaistu;
        private bool bFiltered;
        private bool bkuvausHaettu;
        public bool bPoista;
        public int iTarjNro;

        public Tarjous()
        {
            strKunta = "N/A";
            strTunnus = "N/A";
            strAlkuperainenLinkki = "N/A";
            strTajousDocLinkki = "N/A";
            strTarjousDirLinkki = "N/A";
            strVaihtoehtoLinkki = "N/A";
            strPyynto = "N/A";
            strKuvaus = "N/A";
            strMaaraAika = "31.12.9999 23:59:00";
            strJulkaistu = DateTime.Today.ToString();
            strDataBase = "N/A";
            strFiltered = "false";
            strIlmoitusTyyppi = "N/A";
            strKuvaushaettu = "false";
            bPoista = false;
            strKommentti = "N/A";
            strSuodatettu = "N/A";
            iKiinnostus = 0;
            iTarjNro =++nro;


        }
        public Tarjous(string inKunta, string inTyyppi) : this()
        {

            strKunta = inKunta;
            strIlmoitusTyyppi = inTyyppi;
        }
        public void VaihdaYksikko(string inKunta)
        {
            this.strKunta = inKunta;
        }
        public override string ToString()
        {

            return "{" + strMaaraAika + ": " + strPyynto + " (" + strKunta + ")[" + strDataBase + "]\n" + strKuvaus + "}";// + strTunnus +" = "+ strLinkki;
        }
        public string ToHtmlHakemistoString(bool bShowId)
        {
            //b,i
            String strRetVal = "<li>";


            if (bShowId)
                strRetVal += iTarjNro;
            strRetVal += "<input type=\"checkbox\" id=\"" + iTarjNro + "\" > ";
            if (iKiinnostus == 4) strRetVal += "<b><span style = \"color: red;\">";
            if (iKiinnostus == 3) strRetVal += "<i><span style = \"color: red;\">";
            if (iKiinnostus == 2) strRetVal += "<i>";
            
            strRetVal += strMaaraAika;
            if (iKiinnostus == 4) strRetVal += "</span></b>";
            if (iKiinnostus == 3) strRetVal += "</span></i>";
            if (iKiinnostus == 2) strRetVal += "</i>";
            strRetVal += " <a href = \"#Link_" + iTarjNro + "\">";
            strRetVal += strPyynto;
            if (!strKommentti.Equals("N/A"))
                strRetVal += " (<span style = \"color: red;\">" + strKommentti + "</span> )";
            strRetVal += "</a>";

            strRetVal += "</li>";
            return strRetVal;
        }
        public string ToHtmlHakemistoStringB(bool bChecked)
        {
            String strRetVal = "<li> <input type=\"checkbox\" id=\"" + iTarjNro + "\" > " + strMaaraAika + " <a href = \"#Link_" + iTarjNro + "\">";
            if(bChecked)
                strRetVal = "<li> <input type=\"checkbox\" checked id=\"" + iTarjNro + "\" > " + strMaaraAika + " <a href = \"#Link_" + iTarjNro + "\">";
            strRetVal += strPyynto;
            if (!strKommentti.Equals("N/A"))
                strRetVal += " (<span style = \"color: red;\">" + strKommentti + "</span> )";
            strRetVal += "</a></li>";
            return strRetVal;
        }

        public string ToHtmlKokoString()
        {
            String strRetVal = "<h2>-<input type=\"checkbox\" id=\"" + iTarjNro + "_S\" ><a name=\"Link_" + iTarjNro + "\"></a>-" + strMaaraAika + "--";
            strRetVal += "<a href=\"" + strAlkuperainenLinkki + "\">" + strPyynto + "</a>" + Environment.NewLine;
            strRetVal += "<h3> Tarjousdokumentit </h3>" + Environment.NewLine;
            if(strTajousDocLinkki.Contains("N/A"))
                strRetVal += "<ol><li> Tarjouspyyntö PDF: nä  ei asetettu </li>" + Environment.NewLine;
            else
                strRetVal += "<ol><li> <a href =\"" + strTajousDocLinkki + "\">Tarjouspyyntö PDF: nä  </a> </li>" + Environment.NewLine;
            if (strTarjousDirLinkki.Contains("N/A"))
                strRetVal += "<li> (Liite - hakemisto)  ei asetettu </li> </ol>" + Environment.NewLine;
            else
                strRetVal += "<li> <a href =\"" + strTarjousDirLinkki + "\">Liite - hakemisto  </a></li>    </ol> " + Environment.NewLine;

            strRetVal += "<h3> Kuvaus </h3> " + Environment.NewLine;
            strRetVal += "<p>"+strKuvaus+ "</p>" + Environment.NewLine;
            strRetVal += "<h3> Muuta huomioitavaa </h3> " + Environment.NewLine;
            strRetVal += "<h3> Muut linkit </h3> " + Environment.NewLine;
            strRetVal += "Lähde "+strDataBase + Environment.NewLine;
            strRetVal += "<br>Aito: <a href=\"" + strAlkuperainenLinkki + "\">" + strAlkuperainenLinkki + "</a>" + Environment.NewLine;
            strRetVal += "<br>myös: <a href=\"" + strVaihtoehtoLinkki + "\">" + strVaihtoehtoLinkki + "</a><br>" + Environment.NewLine;

            strRetVal += "<h3> Kommentti </h3> " + Environment.NewLine;
            strRetVal += "<p><span style = \"color: red; \"> "+ strKommentti +"</span><br></p>" + Environment.NewLine;

            strRetVal += "<br>    <a href = \"#Lista\"> Takaisin listaan </a> " + Environment.NewLine;
            strRetVal += " <hr> " + Environment.NewLine;


            return strRetVal;
        }

        public int CompareTo(Tarjous inOther)
        {
            if (inOther == null) return -1;
            if (this.dtMaaraAika < inOther.dtMaaraAika) return -1;
            if (this.dtMaaraAika == inOther.dtMaaraAika)
                return this.strPyynto.CompareTo(inOther.strPyynto);
            return 1;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Tarjous objAsPart = obj as Tarjous;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public bool Equals(Tarjous inTarjous)
        {
            if (inTarjous == null) return false;
            if (!strMaaraAika.Equals(inTarjous.strMaaraAika))
                return false;
            else if (!strPyynto.Equals(inTarjous.strPyynto))
                return false;
            else if (!strKunta.Equals(inTarjous.strKunta))
                return false;
            else
                return true;
        }
        internal List<Tarjous> UudetTarjoukset(List<Tarjous> inKaikki, List<Tarjous> inUudet)
        {
            List<Tarjous> lstUudet = new List<Tarjous>();
            foreach (var inTarjous in inUudet)
            {
                if (!inKaikki.Contains(inTarjous))
                {
                    lstUudet.Add(inTarjous);
                }
            }
            return lstUudet;
        }

        internal Tarjous kyseinen(Tarjous clUusi)
        {
            if (clUusi is null)
                return clStaticMuisti;
            else
                clStaticMuisti = clUusi;
            return null;

        }
    }
}