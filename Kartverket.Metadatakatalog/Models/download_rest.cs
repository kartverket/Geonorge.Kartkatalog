﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Koden er generert av et verktøy.
//     Kjøretidsversjon:4.0.30319.34014
//
//     Endringer i denne filen kan føre til feil virkemåte, og vil gå tapt hvis
//     koden genereres på nytt.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.0.30319.33440.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://skjema.geonorge.no/SOSI/download/0.5")]
[System.Xml.Serialization.XmlRootAttribute("Area", Namespace = "http://skjema.geonorge.no/SOSI/download/0.5", IsNullable = false)]
public partial class AreaType
{

    private string codeField;

    private string typeField;

    private string nameField;

    private ProjectionType[] projectionsField;

    private FormatType[] formatsField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string code
    {
        get
        {
            return this.codeField;
        }
        set
        {
            this.codeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public ProjectionType[] projections
    {
        get
        {
            return this.projectionsField;
        }
        set
        {
            this.projectionsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public FormatType[] formats
    {
        get
        {
            return this.formatsField;
        }
        set
        {
            this.formatsField = value;
        }
    }
}


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://skjema.geonorge.no/SOSI/download/0.5")]
[System.Xml.Serialization.XmlRootAttribute("Area", Namespace = "http://skjema.geonorge.no/SOSI/download/0.5", IsNullable = false)]
public partial class OrderAreaType
{

    private string codeField;

    private string typeField;

    private string nameField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string code
    {
        get
        {
            return this.codeField;
        }
        set
        {
            this.codeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.kxml.no/rest/1.0")]
[System.Xml.Serialization.XmlRootAttribute("Link", Namespace = "http://www.kxml.no/rest/1.0", IsNullable = false)]
public partial class LinkType
{

    private string hrefField;

    private string relField;

    private bool templatedField;

    private bool templatedFieldSpecified;

    private string typeField;

    private string deprecationField;

    private string nameField;

    private string titleField;

    /// <remarks/>
    public string href
    {
        get
        {
            return this.hrefField;
        }
        set
        {
            this.hrefField = value;
        }
    }

    /// <remarks/>
    public string rel
    {
        get
        {
            return this.relField;
        }
        set
        {
            this.relField = value;
        }
    }

    /// <remarks/>
    public bool templated
    {
        get
        {
            return this.templatedField;
        }
        set
        {
            this.templatedField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool templatedSpecified
    {
        get
        {
            return this.templatedFieldSpecified;
        }
        set
        {
            this.templatedFieldSpecified = value;
        }
    }

    /// <remarks/>
    public string type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    public string deprecation
    {
        get
        {
            return this.deprecationField;
        }
        set
        {
            this.deprecationField = value;
        }
    }

    /// <remarks/>
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    public string title
    {
        get
        {
            return this.titleField;
        }
        set
        {
            this.titleField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://skjema.geonorge.no/SOSI/download/0.5")]
[System.Xml.Serialization.XmlRootAttribute("Capabilities", Namespace = "http://skjema.geonorge.no/SOSI/download/0.5", IsNullable = false)]
public partial class CapabilitiesType
{

    private System.Nullable<bool> supportsProjectionSelectionField;

    private System.Nullable<bool> supportsFormatSelectionField;

    private System.Nullable<bool> supportsPolygonSelectionField;

    private System.Nullable<bool> supportsAreaSelectionField;

    private LinkType[] _linksField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public System.Nullable<bool> supportsProjectionSelection
    {
        get
        {
            return this.supportsProjectionSelectionField;
        }
        set
        {
            this.supportsProjectionSelectionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public System.Nullable<bool> supportsFormatSelection
    {
        get
        {
            return this.supportsFormatSelectionField;
        }
        set
        {
            this.supportsFormatSelectionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public System.Nullable<bool> supportsPolygonSelection
    {
        get
        {
            return this.supportsPolygonSelectionField;
        }
        set
        {
            this.supportsPolygonSelectionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public System.Nullable<bool> supportsAreaSelection
    {
        get
        {
            return this.supportsAreaSelectionField;
        }
        set
        {
            this.supportsAreaSelectionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    [System.Xml.Serialization.XmlArrayItemAttribute("_links", Namespace = "http://www.kxml.no/rest/1.0", IsNullable = false)]
    public LinkType[] _links
    {
        get
        {
            return this._linksField;
        }
        set
        {
            this._linksField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://skjema.geonorge.no/SOSI/download/0.5")]
[System.Xml.Serialization.XmlRootAttribute("File", Namespace = "http://skjema.geonorge.no/SOSI/download/0.5", IsNullable = false)]
public partial class FileType
{

    private string downloadUrlField;

    private string fileSizeField;

    private string nameField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string downloadUrl
    {
        get
        {
            return this.downloadUrlField;
        }
        set
        {
            this.downloadUrlField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "integer", IsNullable = true)]
    public string fileSize
    {
        get
        {
            return this.fileSizeField;
        }
        set
        {
            this.fileSizeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://skjema.geonorge.no/SOSI/download/0.5")]
[System.Xml.Serialization.XmlRootAttribute("Format", Namespace = "http://skjema.geonorge.no/SOSI/download/0.5", IsNullable = false)]
public partial class FormatType
{

    private string nameField;

    private string versionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://skjema.geonorge.no/SOSI/download/0.5")]
[System.Xml.Serialization.XmlRootAttribute("Order", Namespace = "http://skjema.geonorge.no/SOSI/download/0.5", IsNullable = false)]
public partial class OrderType
{

    private string emailField;

    private OrderLineType[] orderLinesField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string email
    {
        get
        {
            return this.emailField;
        }
        set
        {
            this.emailField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public OrderLineType[] orderLines
    {
        get
        {
            return this.orderLinesField;
        }
        set
        {
            this.orderLinesField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://skjema.geonorge.no/SOSI/download/0.5")]
[System.Xml.Serialization.XmlRootAttribute("OrderLine", Namespace = "http://skjema.geonorge.no/SOSI/download/0.5", IsNullable = false)]
public partial class OrderLineType
{

    private OrderAreaType[] areasField;

    private FormatType[] formatsField;

    private string metadataUuidField;

    private string coordinatesField;

    private string coordinatesystemField;

    private ProjectionType[] projectionsField;

    /// <summary>
    /// Set selected areas for download
    /// </summary>
    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    [System.Xml.Serialization.XmlArrayItemAttribute("area", IsNullable = false)]
    public OrderAreaType[] areas
    {
        get
        {
            return this.areasField;
        }
        set
        {
            this.areasField = value;
        }
    }

    /// <summary>
    /// Set selected formats to download
    /// </summary>
    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    [System.Xml.Serialization.XmlArrayItemAttribute("format", IsNullable = false)]
    public FormatType[] formats
    {
        get
        {
            return this.formatsField;
        }
        set
        {
            this.formatsField = value;
        }
    }

    /// <summary>
    /// A uniqe reference to datasett from kartkatalog.geonorge.no
    /// </summary>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string metadataUuid
    {
        get
        {
            return this.metadataUuidField;
        }
        set
        {
            this.metadataUuidField = value;
        }
    }

    /// <summary>
    /// If polygon is selected this includes coordinates
    /// </summary>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string coordinates
    {
        get
        {
            return this.coordinatesField;
        }
        set
        {
            this.coordinatesField = value;
        }
    }

    /// <summary>
    /// If polygon is selected this includes coordinate system
    /// </summary>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string coordinatesystem
    {
        get
        {
            return this.coordinatesystemField;
        }
        set
        {
            this.coordinatesystemField = value;
        }
    }

    /// <summary>
    /// Selected projections to download
    /// </summary>
    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    [System.Xml.Serialization.XmlArrayItemAttribute("projection", IsNullable = false)]
    public ProjectionType[] projections
    {
        get
        {
            return this.projectionsField;
        }
        set
        {
            this.projectionsField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://skjema.geonorge.no/SOSI/download/0.5")]
[System.Xml.Serialization.XmlRootAttribute("Projection", Namespace = "http://skjema.geonorge.no/SOSI/download/0.5", IsNullable = false)]
public partial class ProjectionType
{

    private string codeField;

    private string nameField;

    private string codespaceField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string code
    {
        get
        {
            return this.codeField;
        }
        set
        {
            this.codeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string codespace
    {
        get
        {
            return this.codespaceField;
        }
        set
        {
            this.codespaceField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://skjema.geonorge.no/SOSI/download/0.5")]
[System.Xml.Serialization.XmlRootAttribute("OrderReceipt", Namespace = "http://skjema.geonorge.no/SOSI/download/0.5", IsNullable = false)]
public partial class OrderReceiptType
{

    private string referenceNumberField;

    private FileType[] filesField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string referenceNumber
    {
        get
        {
            return this.referenceNumberField;
        }
        set
        {
            this.referenceNumberField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true)]
    [System.Xml.Serialization.XmlArrayItemAttribute("file", IsNullable = false)]
    public FileType[] files
    {
        get
        {
            return this.filesField;
        }
        set
        {
            this.filesField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.33440")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.kxml.no/rest/1.0")]
[System.Xml.Serialization.XmlRootAttribute("LinkListe", Namespace = "http://www.kxml.no/rest/1.0", IsNullable = false)]
public partial class LinkListeType
{

    private LinkType[] _linksField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("_links")]
    public LinkType[] _links
    {
        get
        {
            return this._linksField;
        }
        set
        {
            this._linksField = value;
        }
    }
}
