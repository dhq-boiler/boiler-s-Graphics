using Homura.ORM;
using Homura.ORM.Mapping;
using System;

namespace boilersGraphics.Models;

public class PrivacyPolicyAgreement : EntityBaseObject
{
    private DateTime _DateOfAgreement;
    private DateTime _DateOfEnactment;
    private bool _IsAgree;

    [Column("DateOfEnactment", "NUMERIC", 0)]
    [PrimaryKey]
    [Since(typeof(VersionOrigin))]
    public DateTime DateOfEnactment
    {
        get => _DateOfEnactment;
        set => SetProperty(ref _DateOfEnactment, value);
    }

    [Column("IsAgree", "NUMERIC", 1)]
    [NotNull]
    [Since(typeof(VersionOrigin))]
    public bool IsAgree
    {
        get => _IsAgree;
        set => SetProperty(ref _IsAgree, value);
    }

    [Column("DateOfAgreement", "NUMERIC", 2)]
    [Since(typeof(VersionOrigin))]
    public DateTime DateOfAgreement
    {
        get => _DateOfAgreement;
        set => SetProperty(ref _DateOfAgreement, value);
    }
}