/*****************************************************************
 * Ez az osztály képezi az egyedválasztók õsét. Úgy van 
 * elkészítve, hogy az egyes projektekhez leszármaztatott
 * egyedválasztókban elég csak az egyedtípusokat tartalamzó
 * enumot fölvenni, és a konstruktorban az entityType változót
 * abból példányosítani.
 * A leszármazott egyedtípus enum bitjeinek érdemes ugyanazt a
 * kiosztást adni, mint az adatbázisbeli megfelelõ int mezõnek.
 * EZT A KONTROLT TEHÁT NE PÉLDÁNYOSÍTSUK, CSAK A LESZÁRMAZOTTAIT,
 * MERT ITT NINCSENEK VÁLASZTHATÓ EGYEDTÍPUSOK.
 * 
 * Az osztály úgy kerüli el a közvetlen adatbázis elérést,
 * hogy static delegate-eket hív, amiket a konkrét projektünk
 * inicializálásakor kell egy-egy függvényre ráállítanunk.
 *****************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using KGySoft.Libraries;
using KGySoft.Libraries.ComponentModel;

namespace KGySoft.Controls
{
    #region EntitySelector típusok

    /// <summary>
    /// A függvénytípus, ami megmondja egy egyed nevét azonosító alapján
    /// </summary>
    /// <param name="value">Egyed azonosító</param>
    /// <param name="entityflags">Egyedtípus bitflagek (nem 0 esetén nem találjuk meg az egydet, ha egyik flag sincs az adatbázisban bebilletntve)</param>
    /// <returns>Az egyed neve, vagy valami "Ismeretlen egyed" szöveg</returns>
    public delegate string FEntityNameByID(int value, object entityflags);

    /// <summary>
    /// A függvénytípus, amivel egyedet választhatunk a Browse gomb (mappa ikon) megnyomásakor
    /// </summary>
    /// <param name="sender">A küldõ egyedválasztó komponens</param>
    /// <param name="entityflags">Egyedtípus bitflagek (ilyen típusok választhatók)</param>
    public delegate void FFindEntity(ucEntitySelector sender, object entityflags);

    /// <summary>
    /// A függvénytípus, amivel behívhatjuk az egyedkarbantartást a Maintenance gomb (jegyzettömb ikon) megnyomásakor
    /// </summary>
    /// <param name="sender">A küldõ egyedválasztó komponens</param>
    /// <param name="value">Egyed azonosító</param>
    public delegate void FMaintenanceEntity(ucEntitySelector sender, int value);

    /// <summary>
    /// A függvénytípus, amivel új egyedet hozhatunk létre a New gomb megnyomásakor
    /// </summary>
    /// <param name="sender">A küldõ egyedválasztó komponens</param>
    /// <param name="entityflags">Egyedtípus (új egyed létrehozását érdemes csak olyan komponensen engedélyezni, ahol pontosan egy egyedtípus választható)</param>
    public delegate void FNewEntity(ucEntitySelector sender);

    /// <summary>
    /// A függvénytípus, ami az automatikus választást hajtja végre a szövegmezõrõl való ellépéskor
    /// </summary>
    /// <param name="sender">A küldõ egyedválasztó komponens</param>
    /// <param name="sample">A keresés alapjául szolgáló minta</param>
    /// <param name="entityflags">Egyedtípus bitflagek (ilyen típusok választhatók)</param>
    public delegate void FAutoFindEntity(ucEntitySelector sender, string sample, object entityflags);

    /// <summary>
    /// Természetes/Jogi személy státusz lehetséges értékei
    /// </summary>
    public enum EntityNatures
    {
        All = Constants.AllSelectedValue,
        LegalEntity = 0,
        NaturalPerson = 1
    }

    #endregion

    public partial class ucEntitySelector : ucCustomSelector
    {
        #region Statikus globális változók

        /// <summary>
        /// A függvény, ami megmondja egy egyed nevét azonosító alapján
        /// A projektünkben állítsuk rá egy saját függvényre
        /// </summary>
        public static FEntityNameByID EntityNameByID = null;

        /// <summary>
        /// A függvény, amivel egyedet választhatunk a Browse gomb (mappa ikon) megnyomásakor
        /// A projektünkben állítsuk rá egy saját függvényre
        /// </summary>
        public static FFindEntity FindEntity = null;

        /// <summary>
        /// A függvény, amivel behívhatjuk az egyedkarbantartást a Maintenance gomb (jegyzettömb ikon) megnyomásakor
        /// A projektünkben állítsuk rá egy saját függvényre
        /// </summary>
        public static FMaintenanceEntity MaintenanceEntity = null;

        /// <summary>
        /// A függvény, amivel új egyedet hozhatunk létre a New gomb megnyomásakor
        /// A projektünkben állítsuk rá egy saját függvényre
        /// </summary>
        public static FNewEntity NewEntity = null;

        /// <summary>
        /// A függvény, ami az automatikus választást hajtja végre a szövegmezõrõl való ellépéskor
        /// A projektünkben állítsuk rá egy saját függvényre
        /// </summary>
        public static FAutoFindEntity AutoFindEntity = null;

        #endregion

        #region Objektumváltozók

        protected object entityType; // A leszármazottakban a megfelelõ enum típusból példányosítsuk!

        #endregion

        #region Property-k

        /// <summary>
        /// Választható egyedtípusok (csak az ucEntitySelector leszármazottaiban állítgatható)
        /// </summary>
        [Category("ucEntitySelector")]
        [Description("Választható egyedtípusok (csak az ucEntitySelector leszármazottaiban állítgatható)")]
        [TypeConverter(typeof(FlagsEnumConverter))]
        public virtual object EntityType
        {
            get { return entityType; }
            set { entityType = value; }
        }

        #endregion

        #region Metódusok

        public ucEntitySelector()
        {
            InitializeComponent();
            // Leszármazottban itt lehet egy "entityType = new SajátEnumTípusom();" sor 
        }

        protected override string GetTextByValue(object value)
        {
            if ((int)value > 0 && EntityNameByID != null)
				return EntityNameByID((int)value, entityType);
            else return base.GetTextByValue(value);
        }

        public override void DefaultBrowseClick()
        {
            if (FindEntity != null)
                FindEntity(this, entityType);
 	        else base.DefaultBrowseClick();
        }

        public override void  DefaultEditorClick()
        {
            if (MaintenanceEntity != null)
                if ((int)Value > 0)
                    MaintenanceEntity(this, (int)Value);
                else Dialogs.WarningMessage("No entity selected");
            else base.DefaultEditorClick();
        }

        public override void DefaultNewClick()
        {
            if (NewEntity != null)
                NewEntity(this);
 	        else base.DefaultNewClick();
        }

        public override void DefaultAutoFind(string text)
        {
            if (AutoFindEntity != null)
                AutoFindEntity(this, text, entityType);
            else base.DefaultAutoFind(text);
        }

        #endregion
    }
}

