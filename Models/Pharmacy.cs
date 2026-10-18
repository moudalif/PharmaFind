using PharmaFind.Helpers;

namespace PharmaFind.Models;

/// <summary>Pharmacie partenaire. L'adressage camerounais repose sur ville, arrondissement, quartier et repère.</summary>
public class Pharmacy
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;     // arrondissement
    public string Neighborhood { get; set; } = string.Empty; // quartier
    public string Landmark { get; set; } = string.Empty;     // repère local
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string Phone { get; set; } = string.Empty;        // 9 chiffres, format national
    public string Email { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Pharmacist { get; set; } = string.Empty;   // pharmacien titulaire

    public TimeSpan OpensAt { get; set; } = new(8, 0, 0);
    public TimeSpan ClosesAt { get; set; } = new(20, 0, 0);
    public bool Is24h { get; set; }
    public bool OpenOnSunday { get; set; }
    public bool IsOnDuty { get; set; }                        // pharmacie de garde

    public ValidationStatus Status { get; set; } = ValidationStatus.EnAttente;
    public DateTime RequestedAt { get; set; }
    public string? StatusReason { get; set; }
    public List<string> Documents { get; set; } = [];

    public bool HasCoordinates => Latitude.HasValue && Longitude.HasValue;
    public string LocationText => $"{Neighborhood}, {District}";
    public string FullLocationText => $"{Neighborhood}, {District}, {City}";
    public string PhoneDisplay => PhoneFormatter.Format(Phone);

    public string HoursText
    {
        get
        {
            if (Is24h) return "Ouverte 24 h/24";
            var days = OpenOnSunday ? "7 j/7" : "lun. – sam.";
            return $"{OpensAt:hh\\:mm} – {ClosesAt:hh\\:mm} · {days}";
        }
    }

    public bool IsOpenAt(DateTime moment)
    {
        if (Is24h || IsOnDuty) return true;
        if (moment.DayOfWeek == DayOfWeek.Sunday && !OpenOnSunday) return false;
        var t = moment.TimeOfDay;
        return t >= OpensAt && t < ClosesAt;
    }

    public bool IsOpenNow => IsOpenAt(DateTime.Now);
    public string OpenText => IsOpenNow ? (IsOnDuty && !Is24h ? "Ouverte · de garde" : "Ouverte") : "Fermée";

    public Pharmacy Clone()
    {
        var copy = (Pharmacy)MemberwiseClone();
        copy.Documents = [.. Documents];
        return copy;
    }
}
