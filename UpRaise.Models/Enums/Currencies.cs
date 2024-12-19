using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpRaise.Models.Enums
{
    public enum Currencies: byte
    {
        [Description("United States Dollar")]
        USD = 1,

        [Description("Canada Dollar")]
        CAD = 2,

        /*
        [Description("United Arab Emirates Dirham")]
        AED=1,

        [Description("Afghanistan Afghani")]
        AFN=2,

        [Description("Albania Lek")]
        ALL=3,
        [Description("Armenia Dram")]
        AMD=4,
        [Description("Netherlands Antilles Guilder")]
        ANG=,
        [Description("Angola Kwanza")]
        AOA =,
        [Description("Argentina Peso")]
        ARS =,
        [Description("Australia Dollar")]
        AUD =,
        [Description("Aruba Guilder")]
        AWG =,
        [Description("Azerbaijan New Manat")]
        AZN =,
        [Description("Bosnia and Herzegovina Convertible Marka")]
        BAM =,
        [Description("Barbados Dollar")]
        BBD =,
        [Description("Bangladesh Taka")]
        BDT =,
        [Description("Bulgaria Lev")]
        BGN =,
        [Description("Bahrain Dinar")]
        BHD =,
        [Description("Burundi Franc")]
        BIF =,
        [Description("Bermuda Dollar")]
        BMD =,
        [Description("Brunei Darussalam Dollar")]
        BND =,
        [Description("Bolivia Bolíviano")]
        BOB =,
        [Description("Brazil Real")]
        BRL =,
        [Description("Bahamas Dollar")]
        BSD =,
        [Description("Bhutan Ngultrum")]
        BTN =,
        [Description("Botswana Pula")]
        BWP =,
        [Description("Belarus Ruble")]
        BYR =,
        [Description("Belize Dollar")]
        BZD =,
        [Description("Congo/Kinshasa Franc")]
        CDF =,
        [Description("Switzerland Franc")]
        CHF =,
        [Description("Chile Peso")]
        CLP =,
        [Description("China Yuan Renminbi")]
        CNY =,
        [Description("Colombia Peso")]
        COP =,
        [Description("Costa Rica Colon")]
        CRC,
        [Description("Cuba Convertible Peso")]
        CUC,
        [Description("Cuba Peso")]
        CUP,
        [Description("Cape Verde Escudo")]
        CVE,
        [Description("Czech Republic Koruna")]
        CZK,
        [Description("Djibouti Franc")]
        DJF,
        [Description("Denmark Krone")]
        DKK,
        
        [Description]
        DOP("Dominican Republic Peso") ,
        [Description]
        DZD("Algeria Dinar")  ,
        [Description]
        EGP("Egypt Pound"),
        [Description]
        ERN("Eritrea Nakfa"),
        [Description]
        ETB("Ethiopia Birr"),
        [Description]
        EUR("Euro Member Countries"),
        [Description]
        FJD("Fiji Dollar"),
        [Description]
        FKP("Falkland Islands (Malvinas) Pound"),
        [Description("United Kingdom Pound")]
        GBP,
        [Description]
        GEL("Georgia Lari"),
        [Description]
        GGP("Guernsey Pound"),
        [Description]
        GHS("Ghana Cedi"),
        [Description]
        GIP("Gibraltar Pound"),
        [Description]
        GMD("Gambia Dalasi"),
        [Description]
        GNF("Guinea Franc"),
        [Description]
        GTQ("Guatemala Quetzal"),
        [Description]
        GYD("Guyana Dollar"),
        [Description("Hong Kong Dollar")]
        HKD,
        [Description]
        HNL("Honduras Lempira"),
        [Description]
        HRK("Croatia Kuna"),
        [Description]
        HTG("Haiti Gourde"),
        [Description]
        HUF("Hungary Forint"),
        [Description]
        IDR("Indonesia Rupiah"),
        [Description]
        ILS("Israel Shekel"),
        [Description]
        IMP("Isle of Man Pound"),
        [Description("India Rupee")]
        INR,
        [Description]
        IQD("Iraq Dinar"),
        [Description]
        IRR("Iran Rial"),
        [Description]
        ISK("Iceland Krona"),
        [Description]
        JEP("Jersey Pound"),
        [Description]
        JMD("Jamaica Dollar"),
        [Description]
        JOD("Jordan Dinar"),
        [Description]
        JPY("Japan Yen"),
        [Description]
        KES("Kenya Shilling"),
        [Description]
        KGS("Kyrgyzstan Som"),
        [Description]
        KHR("Cambodia Riel"),
        [Description]
        KMF("Comoros Franc"),
        [Description]
        KPW("Korea (North) Won"),
        [Description]
        KRW("Korea (South) Won"),
        [Description]
        KWD("Kuwait Dinar"),
        [Description]
        KYD("Cayman Islands Dollar"),
        [Description]
        KZT("Kazakhstan Tenge"),
        [Description]
        LAK("Laos Kip"),
        [Description]
        LBP("Lebanon Pound"),
        [Description]
        LKR("Sri Lanka Rupee"),
        [Description]
        LRD("Liberia Dollar"),
        [Description]
        LSL("Lesotho Loti"),
        [Description]
        LYD("Libya Dinar"),
        [Description]
        MAD("Morocco Dirham"),
        [Description]
        MDL("Moldova Leu"),
        [Description]
        MGA("Madagascar Ariary"),
        [Description]
        MKD("Macedonia Denar"),
        [Description]
        MMK("Myanmar (Burma) Kyat"),
        [Description]
        MNT("Mongolia Tughrik"),
        [Description]
        MOP("Macau Pataca"),
        [Description]
        MRO("Mauritania Ouguiya"),
        [Description]
        MUR("Mauritius Rupee"),
        [Description]
        MVR("Maldives (Maldive Islands) Rufiyaa"),
        [Description]
        MWK("Malawi Kwacha"),
        [Description]
        MXN("Mexico Peso"),
        [Description]
        MYR("Malaysia Ringgit"),
        [Description]
        MZN("Mozambique Metical"),
        [Description]
        NAD("Namibia Dollar"),
        [Description]
        NGN("Nigeria Naira"),
        [Description]
        NIO("Nicaragua Cordoba"),
        [Description]
        NOK("Norway Krone"),
        [Description]
        NPR("Nepal Rupee"),
        [Description]
        NZD("New Zealand Dollar"),
        [Description]
        OMR("Oman Rial"),
        [Description]
        PAB("Panama Balboa"),
        [Description]
        PEN("Peru Sol"),
        [Description]
        PGK("Papua New Guinea Kina"),
        [Description]
        PHP("Philippines Peso"),
        [Description]
        PKR("Pakistan Rupee"),
        [Description]
        PLN("Poland Zloty"),
        [Description]
        PYG("Paraguay Guarani"),
        [Description]
        QAR("Qatar Riyal"),
        [Description]
        RON("Romania New Leu"),
        [Description]
        RSD("Serbia Dinar"),
        [Description("Russia Ruble")]
        RUB,
        [Description]
        RWF("Rwanda Franc"),
        [Description]
        SAR("Saudi Arabia Riyal"),
        [Description]
        SBD("Solomon Islands Dollar"),
        [Description]
        SCR("Seychelles Rupee"),
        [Description]
        SDG("Sudan Pound"),
        [Description]
        SEK("Sweden Krona"),
        [Description]
        SGD("Singapore Dollar"),
        [Description]
        SHP("Saint Helena Pound"),
        [Description]
        SLL("Sierra Leone Leone"),
        [Description]
        SOS("Somalia Shilling"),
        [Description]
        SPL("Seborga Luigino"),
        [Description]
        SRD("Suriname Dollar"),
        [Description]
        STD("São Tomé and Príncipe Dobra"),
        [Description]
        SVC("El Salvador Colon"),
        [Description]
        SYP("Syria Pound"),
        [Description]
        SZL("Swaziland Lilangeni"),
        [Description]
        THB("Thailand Baht"),
        [Description]
        TJS("Tajikistan Somoni"),
        [Description]
        [Description]
        TMT("Turkmenistan Manat"),
        [Description]
        TND("Tunisia Dinar"),
        [Description]
        TOP("Tonga Pa'anga"),
        [Description("Turkey Lira")]
        TRY,
        [Description]
        TTD("Trinidad and Tobago Dollar"),
        [Description]
        TVD("Tuvalu Dollar"),
        [Description]
        TWD("Taiwan New Dollar"),
        [Description]
        TZS("Tanzania Shilling"),
        [Description]
        UAH("Ukraine Hryvnia"),
        [Description]
        UGX("Uganda Shilling"),
        [Description]
        UYU("Uruguay Peso"),
        [Description]
        UZS("Uzbekistan Som"),
        [Description]
        VEF("Venezuela Bolivar"),
        [Description("Viet Nam Dong")]
        VND,
        [Description]
        VUV("Vanuatu Vatu"),
        [Description]
        WST("Samoa Tala"),
        [Description]
        XAF("Communauté Financière Africaine (BEAC) CFA Franc BEAC"),
        [Description]
        XCD("East Caribbean Dollar"),
        [Description]
        XDR("International Monetary Fund (IMF) Special Drawing Rights"),
        [Description]
        XOF("Communauté Financière Africaine (BCEAO) Franc"),
        [Description]
        XPF("Comptoirs Français du Pacifique (CFP) Franc"),
        [Description]
        YER("Yemen Rial"),
        [Description("South Africa Rand")]
        ZAR,
        [Description]
        ZMW("Zambia Kwacha"),
        [Description]
        ZWD("Zimbabwe Dollar");
        */
    }
}
