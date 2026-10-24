using AutoHub.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoHub.Data;

public static class AutoHubSeeder
{
    public static async Task SeedAsync(AutoHubDbContext db)
    {
        var hasBrands = await db.Brands.AnyAsync();
        var hasMedia = await db.MediaAssets.AnyAsync();
        var hasVariants = await db.VehicleVariants.AnyAsync();

        if (hasBrands && hasMedia && hasVariants) return;

        if (!hasBrands)
        {
            var bodyTypes = await SeedBodyTypes(db);
            var fuelTypes = await SeedFuelTypes(db);
            var transTypes = await SeedTransmissionTypes(db);
            var driveTypes = await SeedDriveTypes(db);
            var colors = await SeedColors(db);
            var brands = await SeedBrands(db);
            await db.SaveChangesAsync();

            var models = await SeedVehicleModels(db, brands, bodyTypes);
            await db.SaveChangesAsync();

            var variants = await SeedVehicleVariants(db, models, fuelTypes, transTypes, driveTypes);
            await db.SaveChangesAsync();

            await SeedSpecifications(db, variants);
            await SeedVariantColors(db, variants, colors);
            await SeedMedia(db, variants);
            await db.SaveChangesAsync();
        }
        else if (!hasMedia || !hasVariants)
        {
            var variants = await db.VehicleVariants.ToListAsync();
            if (variants.Count > 0)
            {
                if (!hasMedia)
                {
                    await SeedMedia(db, variants);
                    await db.SaveChangesAsync();
                }
            }
        }
    }

    private static async Task<Dictionary<string, BodyTypeEntity>> SeedBodyTypes(AutoHubDbContext db)
    {
        var data = new[] { ("Sedan", "sedan"), ("SUV", "suv"), ("Coupe", "coupe"), ("Hatchback", "hatchback"), ("MPV", "mpv"), ("Pickup", "pickup"), ("Wagon", "wagon"), ("Convertible", "convertible") };
        var dict = new Dictionary<string, BodyTypeEntity>();
        foreach (var (name, slug) in data) { var e = new BodyTypeEntity { Name = name, Slug = slug }; db.BodyTypes.Add(e); dict[slug] = e; }
        await db.SaveChangesAsync();
        return dict;
    }

    private static async Task<Dictionary<string, FuelTypeEntity>> SeedFuelTypes(AutoHubDbContext db)
    {
        var data = new[] { ("Petrol", "petrol"), ("Diesel", "diesel"), ("Hybrid", "hybrid"), ("PHEV", "phev"), ("EV", "ev") };
        var dict = new Dictionary<string, FuelTypeEntity>();
        foreach (var (name, slug) in data) { var e = new FuelTypeEntity { Name = name, Slug = slug }; db.FuelTypes.Add(e); dict[slug] = e; }
        await db.SaveChangesAsync();
        return dict;
    }

    private static async Task<Dictionary<string, TransmissionTypeEntity>> SeedTransmissionTypes(AutoHubDbContext db)
    {
        var data = new[] { ("AT", "at"), ("CVT", "cvt"), ("DCT", "dct"), ("Manual", "manual") };
        var dict = new Dictionary<string, TransmissionTypeEntity>();
        foreach (var (name, slug) in data) { var e = new TransmissionTypeEntity { Name = name, Slug = slug }; db.TransmissionTypes.Add(e); dict[slug] = e; }
        await db.SaveChangesAsync();
        return dict;
    }

    private static async Task<Dictionary<string, DriveTypeEntity>> SeedDriveTypes(AutoHubDbContext db)
    {
        var data = new[] { ("FWD", "fwd"), ("RWD", "rwd"), ("AWD", "awd"), ("4WD", "4wd") };
        var dict = new Dictionary<string, DriveTypeEntity>();
        foreach (var (name, slug) in data) { var e = new DriveTypeEntity { Name = name, Slug = slug }; db.DriveTypes.Add(e); dict[slug] = e; }
        await db.SaveChangesAsync();
        return dict;
    }

    private static async Task<Dictionary<string, ColorEntity>> SeedColors(AutoHubDbContext db)
    {
        var data = new (string name, string hex, string finish)[]
        {
            ("Alpine White", "#FFFFFF", "Solid"), ("Black Sapphire", "#0A0A0A", "Metallic"), ("Mineral Grey", "#6B6D70", "Metallic"),
            ("Phytonic Blue", "#1B3A5C", "Metallic"), ("Cavansite Blue", "#1A3C6E", "Metallic"), ("Manhattan Green", "#2D4A3E", "Metallic"),
            ("Portimao Blue", "#0047AB", "Metallic"), ("Polar White", "#F5F5F5", "Solid"), ("Obsidian Black", "#0B0B0B", "Metallic"),
            ("Selenite Grey", "#8C8C8C", "Metallic"), ("Iridium Silver", "#C0C0C0", "Metallic"), ("Mythos Black", "#0D0D0D", "Metallic"),
            ("Navarra Blue", "#1E3A5F", "Metallic"), ("Daytona Grey", "#5A5A5A", "Metallic"), ("Ibis White", "#FAFAFA", "Solid"),
            ("Platinum White Pearl", "#F0EDE8", "Pearl"), ("Attitude Black", "#111111", "Solid"), ("Soul Red Crystal", "#8B0000", "Metallic"),
            ("Rhodium White", "#F8F8F8", "Metallic"), ("Sonic Titanium", "#8B8682", "Metallic"), ("Eminent White Pearl", "#FAFAFA", "Pearl"),
            ("Graphite Grey", "#383838", "Metallic"), ("Jet Black", "#0A0A0A", "Solid"), ("Carrara White", "#F5F0EB", "Metallic"),
            ("Guards Red", "#C41E3A", "Solid"), ("Racing Yellow", "#FFD700", "Solid"), ("Moonlight Blue", "#1C2E4A", "Metallic"),
            ("Onyx Black", "#050505", "Metallic"), ("Crystal White Pearl", "#F5F5F0", "Pearl"), ("Thunder Grey", "#4A4A4A", "Metallic"),
            ("Denim Blue", "#2B3E5A", "Metallic"), ("Ingot Silver", "#B0B0B0", "Metallic"), ("True Blue", "#1A5276", "Solid"),
            ("Absolute Black", "#050505", "Metallic"), ("Machine Grey", "#4A4A4A", "Metallic"), ("Snowflake White Pearl", "#F0F0F0", "Pearl"),
        };
        var dict = new Dictionary<string, ColorEntity>();
        foreach (var (name, hex, finish) in data)
        {
            var e = new ColorEntity { Name = name, ColorCode = hex, HexCode = hex, FinishType = finish };
            db.Colors.Add(e);
            if (!dict.ContainsKey(name)) dict[name] = e;
        }
        await db.SaveChangesAsync();
        return dict;
    }

    private static async Task<Dictionary<string, BrandEntity>> SeedBrands(AutoHubDbContext db)
    {
        var data = new[] {
            ("BMW", "bmw", "Germany", "https://www.bmw.com", "/Images/Brands/bmw.svg"),
            ("Mercedes-Benz", "mercedes-benz", "Germany", "https://www.mercedes-benz.com", "/Images/Brands/mercedes-benz.svg"),
            ("Audi", "audi", "Germany", "https://www.audi.com", "/Images/Brands/audi.svg"),
            ("Toyota", "toyota", "Japan", "https://www.toyota.com", "/Images/Brands/toyota.svg"),
            ("Honda", "honda", "Japan", "https://www.honda.com", "/Images/Brands/honda.svg"),
            ("Mazda", "mazda", "Japan", "https://www.mazda.com", "/Images/Brands/mazda.svg"),
            ("Lexus", "lexus", "Japan", "https://www.lexus.com", "/Images/Brands/lexus.svg"),
            ("Porsche", "porsche", "Germany", "https://www.porsche.com", "/Images/Brands/porsche.svg"),
            ("Volvo", "volvo", "Sweden", "https://www.volvo.com", "/Images/Brands/volvo.svg"),
            ("Ford", "ford", "USA", "https://www.ford.com", "/Images/Brands/ford.svg"),
            ("VinFast", "vinfast", "Vietnam", "https://www.vinfast.vn", "/Images/Brands/vinfast.svg"),
            ("Hyundai", "hyundai", "South Korea", "https://www.hyundai.com", "/Images/Brands/hyundai.svg"),
            ("Kia", "kia", "South Korea", "https://www.kia.com", "/Images/Brands/kia.svg"),
            ("Volkswagen", "volkswagen", "Germany", "https://www.volkswagen.com", "/Images/Brands/volkswagen.svg"),
            ("Nissan", "nissan", "Japan", "https://www.nissan-global.com", "/Images/Brands/nissan.svg"),
        };
        var dict = new Dictionary<string, BrandEntity>();
        foreach (var (name, slug, country, url, logo) in data)
        {
            var e = new BrandEntity { Name = name, Slug = slug, Country = country, WebsiteUrl = url, LogoPath = logo };
            db.Brands.Add(e);
            dict[slug] = e;
        }
        await db.SaveChangesAsync();
        return dict;
    }

    private static async Task<Dictionary<string, VehicleModelEntity>> SeedVehicleModels(AutoHubDbContext db, Dictionary<string, BrandEntity> brands, Dictionary<string, BodyTypeEntity> bt)
    {
        var data = new[] {
            ("bmw", "3 Series", "3-series", "sedan", "G20", 2019, 2025),
            ("bmw", "5 Series", "5-series", "sedan", "G60", 2023, 2025),
            ("bmw", "X3", "x3", "suv", "G01", 2018, 2024),
            ("bmw", "X5", "x5", "suv", "G05", 2019, 2025),
            ("mercedes-benz", "C-Class", "c-class", "sedan", "W206", 2022, 2025),
            ("mercedes-benz", "E-Class", "e-class", "sedan", "W214", 2024, 2025),
            ("mercedes-benz", "GLC", "glc", "suv", "X254", 2023, 2025),
            ("audi", "A4", "a4", "sedan", "B9", 2017, 2024),
            ("audi", "A6", "a6", "sedan", "C8", 2019, 2025),
            ("audi", "Q5", "q5", "suv", "FY", 2018, 2024),
            ("toyota", "Camry", "camry", "sedan", "XV70", 2019, 2025),
            ("toyota", "Corolla Cross", "corolla-cross", "suv", "XU10", 2020, 2025),
            ("toyota", "Fortuner", "fortuner", "suv", "AN150", 2020, 2024),
            ("honda", "Civic", "civic", "sedan", "FE/FG", 2022, 2025),
            ("honda", "CR-V", "cr-v", "suv", "RW", 2023, 2025),
            ("mazda", "CX-5", "cx-5", "suv", "KF", 2017, 2025),
            ("mazda", "Mazda3", "mazda3", "hatchback", "BP", 2019, 2025),
            ("lexus", "ES", "es", "sedan", "XV70", 2019, 2025),
            ("lexus", "RX", "rx", "suv", "AL30", 2023, 2025),
            ("porsche", "911", "911", "coupe", "992", 2020, 2025),
            ("porsche", "Cayenne", "cayenne", "suv", "E3", 2019, 2025),
            ("volvo", "XC60", "xc60", "suv", "II", 2018, 2025),
            ("volvo", "S90", "s90", "sedan", "II", 2017, 2025),
            ("ford", "Ranger", "ranger", "pickup", "PJ", 2022, 2025),
            ("ford", "Everest", "everest", "suv", "U704", 2022, 2025),
            ("vinfast", "VF8", "vf8", "suv", "VF8", 2023, 2025),
            ("hyundai", "Tucson", "tucson", "suv", "NX4", 2021, 2025),
            ("kia", "Sportage", "sportage", "suv", "NQ5", 2022, 2025),
            ("volkswagen", "Tiguan", "tiguan", "suv", "AD/BT", 2021, 2025),
            ("nissan", "X-Trail", "x-trail", "suv", "T33", 2021, 2025),
        };
        var dict = new Dictionary<string, VehicleModelEntity>();
        foreach (var (brandSlug, name, slug, btSlug, gen, yearFrom, yearTo) in data)
        {
            var e = new VehicleModelEntity
            {
                BrandId = brands[brandSlug].BrandId,
                BodyTypeId = bt.TryGetValue(btSlug, out var bte) ? bte.BodyTypeId : null,
                Name = name,
                Slug = slug,
                Generation = gen,
                ModelYearFrom = yearFrom,
                ModelYearTo = yearTo,
            };
            db.VehicleModels.Add(e);
            dict[slug] = e;
        }
        await db.SaveChangesAsync();
        return dict;
    }

    private static async Task<List<VehicleVariantEntity>> SeedVehicleVariants(AutoHubDbContext db, Dictionary<string, VehicleModelEntity> models, Dictionary<string, FuelTypeEntity> fuels, Dictionary<string, TransmissionTypeEntity> trans, Dictionary<string, DriveTypeEntity> drives)
    {
        var variants = new List<VehicleVariantEntity>();
        void Add(string m, string name, string slug, decimal price, string fuel, string tran, string drive, int seats, bool feat, bool nw, bool pop, string? desc, int? year = null, string? market = null)
        {
            if (!models.ContainsKey(m)) return;
            variants.Add(new VehicleVariantEntity { ModelId = models[m].ModelId, VariantName = name, Slug = slug, BasePrice = price, CurrencyCode = "VND", FuelTypeId = fuels[fuel].FuelTypeId, TransmissionTypeId = trans[tran].TransmissionTypeId, DriveTypeId = drives[drive].DriveTypeId, SeatCount = seats, IsFeatured = feat, IsNewArrival = nw, IsPopular = pop, Description = desc, ModelYear = year, MarketCode = market });
        }

        Add("3-series", "320i", "bmw-3-series-320i", 1699000000m, "petrol", "at", "rwd", 5, true, false, true, "BMW 320i voi dong co TwinPower Turbo 2.0L, cong suat 184 ma luc.", 2024, "VN");
        Add("3-series", "330i M Sport", "bmw-3-series-330i-m-sport", 2299000000m, "petrol", "at", "rwd", 5, false, true, false, "BMW 330i M Sport voi dong co TwinPower Turbo 2.0L, cong suat 258 ma luc.", 2024, "VN");
        Add("5-series", "520i Luxury", "bmw-5-series-520i", 2589000000m, "petrol", "at", "rwd", 5, false, true, false, "BMW 520i Luxury voi thiet ke moi nhat, noi that cao cap.", 2024, "VN");
        Add("x3", "xDrive20d", "bmw-x3-xdrive20d", 2499000000m, "diesel", "at", "awd", 5, true, false, false, "BMW X3 xDrive20d voi dong co diesel turbodiesel 2.0L.", 2024, "VN");
        Add("x5", "xDrive40i", "bmw-x5-xdrive40i", 4199000000m, "petrol", "at", "awd", 5, true, false, true, "BMW X5 xDrive40i voi dong co I6 3.0L, cong suat 340 ma luc.", 2024, "VN");
        Add("c-class", "C 200", "mercedes-c-class-c200", 1599000000m, "petrol", "at", "rwd", 5, true, false, true, "Mercedes-Benz C 200 voi dong co turbo 1.5L mild hybrid.", 2024, "VN");
        Add("c-class", "C 300 AMG Line", "mercedes-c-class-c300-amg", 2099000000m, "petrol", "at", "rwd", 5, false, true, false, "Mercedes-Benz C 300 AMG Line voi dong co turbo 2.0L.", 2024, "VN");
        Add("e-class", "E 200", "mercedes-e-class-e200", 2130000000m, "petrol", "at", "rwd", 5, false, true, false, "Mercedes-Benz E 200 voi thiet ke moi nhat, noi that sang trong.", 2024, "VN");
        Add("glc", "GLC 200 4MATIC", "mercedes-glc-200-4matic", 2299000000m, "petrol", "at", "awd", 5, true, false, false, "Mercedes-Benz GLC 200 4MATIC voi dong co turbo 2.0L.", 2024, "VN");
        Add("a4", "45 TFSI", "audi-a4-45-tfsi", 1650000000m, "petrol", "at", "fwd", 5, true, false, false, "Audi A4 45 TFSI voi dong co turbo 2.0L, cong suat 252 ma luc.", 2024, "VN");
        Add("a6", "45 TFSI quattro", "audi-a6-45-tfsi-quattro", 2450000000m, "petrol", "at", "awd", 5, false, true, false, "Audi A6 45 TFSI quattro voi he thong dan dong 4 banh.", 2024, "VN");
        Add("q5", "45 TFSI quattro", "audi-q5-45-tfsi-quattro", 2690000000m, "petrol", "at", "awd", 5, true, false, false, "Audi Q5 45 TFSI quattro SUV the thao voi he thong quattro.", 2024, "VN");
        Add("camry", "2.5Q", "toyota-camry-25q", 1220000000m, "petrol", "at", "fwd", 5, true, false, true, "Toyota Camry 2.5Q voi dong co 2.5L Dynamic Force.", 2024, "VN");
        Add("corolla-cross", "1.8V", "toyota-corolla-cross-18v", 730000000m, "petrol", "cvt", "fwd", 5, false, false, true, "Toyota Corolla Cross 1.8V voi dong co 1.8L, hop so CVT.", 2024, "VN");
        Add("fortuner", "2.4 VRZ", "toyota-fortuner-24-vrz", 1190000000m, "diesel", "at", "4wd", 7, false, false, false, "Toyota Fortuner 2.4 VRZ diesel turbodiesel.", 2024, "VN");
        Add("civic", "1.5 RS", "honda-civic-15rs", 889000000m, "petrol", "cvt", "fwd", 5, true, false, true, "Honda Civic 1.5 RS voi dong co turbo 1.5L.", 2024, "VN");
        Add("cr-v", "1.5 L", "honda-cr-v-15-l", 1099000000m, "petrol", "cvt", "fwd", 5, false, true, false, "Honda CR-V 1.5L turbo, noi that rong rai.", 2024, "VN");
        Add("cx-5", "2.5 Turbo", "mazda-cx5-25t", 899000000m, "petrol", "at", "awd", 5, true, false, false, "Mazda CX-5 2.5 Turbo voi dong co Skyactiv-G 2.5L turbo.", 2024, "VN");
        Add("mazda3", "2.0 Premium", "mazda3-20-premium", 599000000m, "petrol", "at", "fwd", 5, false, false, false, "Mazda3 2.0 Premium voi thiet ke Kodo, noi that cao cap.", 2024, "VN");
        Add("es", "300h", "lexus-es-300h", 3140000000m, "hybrid", "cvt", "fwd", 5, true, false, false, "Lexus ES 300h voi he thong hybrid, tiet kiem nhien lieu.", 2024, "VN");
        Add("rx", "350h", "lexus-rx-350h", 4290000000m, "hybrid", "cvt", "awd", 5, false, true, false, "Lexus RX 350h hybrid SUV cao cap.", 2024, "VN");
        Add("911", "Carrera", "porsche-911-carrera", 7290000000m, "petrol", "dct", "rwd", 4, true, false, false, "Porsche 911 Carrera voi dong co boxer 3.0L twin-turbo.", 2024, "VN");
        Add("cayenne", "Cayenne", "porsche-cayenne", 5490000000m, "petrol", "at", "awd", 5, false, true, false, "Porsche Cayenne SUV hieu nang cao voi dong co V6 twin-turbo.", 2024, "VN");
        Add("xc60", "B5 AWD", "volvo-xc60-b5-awd", 2269000000m, "petrol", "at", "awd", 5, true, false, false, "Volvo XC60 B5 AWD voi dong co mild hybrid.", 2024, "VN");
        Add("s90", "B5 AWD", "volvo-s90-b5-awd", 2399000000m, "petrol", "at", "awd", 5, false, false, false, "Volvo S90 B5 AWD sedan sang trong.", 2024, "VN");
        Add("ranger", "2.0L Bi-Turbo", "ford-ranger-20-bi-turbo", 966000000m, "diesel", "at", "4wd", 5, true, false, true, "Ford Ranger 2.0L Bi-Turbo, cong suat 213 ma luc.", 2024, "VN");
        Add("everest", "2.0L Bi-Turbo", "ford-everest-20-bi-turbo", 1099000000m, "diesel", "at", "4wd", 7, false, false, true, "Ford Everest 2.0L Bi-Turbo, SUV 7 cho.", 2024, "VN");
        Add("vf8", "VF8 Eco", "vinfast-vf8-eco", 1090000000m, "ev", "at", "awd", 5, false, true, false, "VinFast VF8 Eco - SUV dien doi sang voi tam hoat dong 420km.", 2024, "VN");
        Add("tucson", "2.0 Premium", "hyundai-tucson-20-premium", 1020000000m, "petrol", "at", "fwd", 5, false, false, true, "Hyundai Tucson 2.0 Premium - SUV pho bien voi dong co 2.0L.", 2024, "VN");
        Add("sportage", "1.6T GT-Line", "kia-sportage-16t-gtline", 999000000m, "petrol", "dct", "fwd", 5, false, true, false, "Kia Sportage 1.6T GT-Line - SUV phong cach voi dong co turbo.", 2024, "VN");
        Add("tiguan", "2.0 TSI", "volkswagen-tiguan-20-tsi", 1299000000m, "petrol", "dct", "fwd", 5, false, false, false, "Volkswagen Tiguan 2.0 TSI - SUV sang trong voi dong co TSI.", 2024, "VN");
        Add("x-trail", "2.5 SV", "nissan-x-trail-25-sv", 1049000000m, "petrol", "cvt", "fwd", 5, false, false, false, "Nissan X-Trail 2.5 SV - SUV da nang voi dong co 2.5L.", 2024, "VN");

        foreach (var v in variants) db.VehicleVariants.Add(v);
        await db.SaveChangesAsync();
        return variants;
    }

    private static async Task SeedSpecifications(AutoHubDbContext db, List<VehicleVariantEntity> variants)
    {
        var s = new Dictionary<string, (string engine, int? disp, int? cyl, int? hp, int? torque, decimal? accel, int? top, decimal? combined, int? tank, int len, int w, int h, int wb, int gc)>
        {
            ["bmw-3-series-320i"] = ("TwinPower Turbo I4", 1998, 4, 184, 300, 7.1m, 240, 6.2m, 59, 4716, 1826, 1440, 2851, 147),
            ["bmw-3-series-330i-m-sport"] = ("TwinPower Turbo I4", 1998, 4, 258, 400, 5.6m, 250, 6.5m, 59, 4716, 1826, 1440, 2851, 147),
            ["bmw-5-series-520i"] = ("TwinPower Turbo I4", 1998, 4, 190, 310, 7.0m, 240, 6.4m, 60, 5060, 1900, 1515, 2995, 144),
            ["bmw-x3-xdrive20d"] = ("TwinPower Turbo I4 Diesel", 1995, 4, 190, 400, 8.0m, 213, 5.7m, 67, 4722, 1890, 1676, 2864, 204),
            ["bmw-x5-xdrive40i"] = ("TwinPower Turbo I6", 2998, 6, 340, 450, 5.5m, 250, 8.5m, 83, 4922, 2004, 1745, 2975, 209),
            ["mercedes-c-class-c200"] = ("M254 I4 Turbo + ISG", 1496, 4, 204, 300, 7.3m, 240, 6.1m, 66, 4751, 1820, 1438, 2865, 147),
            ["mercedes-c-class-c300-amg"] = ("M254 I4 Turbo + ISG", 1999, 4, 258, 400, 5.9m, 255, 6.5m, 66, 4751, 1820, 1438, 2865, 147),
            ["mercedes-e-class-e200"] = ("M254 I4 Turbo + ISG", 1999, 4, 204, 320, 7.5m, 240, 6.3m, 66, 4949, 1880, 1468, 2961, 147),
            ["mercedes-glc-200-4matic"] = ("M254 I4 Turbo + ISG", 1999, 4, 204, 320, 7.9m, 222, 7.0m, 66, 4764, 1890, 1640, 2888, 183),
            ["audi-a4-45-tfsi"] = ("EA888 I4 Turbo", 1984, 4, 252, 370, 5.9m, 250, 6.4m, 54, 4762, 1847, 1433, 2825, 147),
            ["audi-a6-45-tfsi-quattro"] = ("EA888 I4 Turbo", 1984, 4, 265, 370, 5.6m, 250, 6.7m, 73, 4939, 1886, 1457, 2924, 149),
            ["audi-q5-45-tfsi-quattro"] = ("EA888 I4 Turbo", 1984, 4, 265, 370, 5.7m, 240, 7.2m, 70, 4680, 1893, 1659, 2819, 179),
            ["toyota-camry-25q"] = ("A25A-FKS I4", 2487, 4, 207, 243, 8.3m, 210, 6.0m, 60, 4885, 1840, 1445, 2825, 145),
            ["toyota-corolla-cross-18v"] = ("2ZR-FBE I4", 1798, 4, 140, 177, 10.2m, 180, 6.5m, 47, 4695, 1820, 1620, 2660, 161),
            ["toyota-fortuner-24-vrz"] = ("1GD-FTV I4 Diesel", 2393, 4, 150, 400, 11.2m, 175, 7.6m, 80, 4795, 1855, 1835, 2745, 219),
            ["honda-civic-15rs"] = ("L15B7 I4 Turbo", 1498, 4, 178, 240, 8.5m, 200, 5.8m, 47, 4674, 1802, 1415, 2735, 134),
            ["honda-cr-v-15-l"] = ("L15BE I4 Turbo", 1498, 4, 190, 243, 9.0m, 200, 6.7m, 53, 4703, 1866, 1680, 2701, 198),
            ["mazda-cx5-25t"] = ("Skyactiv-G 2.5 Turbo I4", 2488, 4, 233, 433, 6.8m, 208, 7.3m, 58, 4575, 1845, 1690, 2700, 200),
            ["mazda3-20-premium"] = ("Skyactiv-G 2.0 I4", 1998, 4, 155, 200, 8.2m, 203, 5.8m, 51, 4660, 1795, 1440, 2725, 135),
            ["lexus-es-300h"] = ("2AR-FXE I4 Hybrid", 2487, 4, 218, 221, 8.9m, 180, 4.7m, 65, 4975, 1865, 1445, 2870, 147),
            ["lexus-rx-350h"] = ("2GR-FXS V6 Hybrid", 3456, 6, 371, 460, 6.2m, 200, 5.7m, 65, 4890, 1920, 1695, 2850, 188),
            ["porsche-911-carrera"] = ("3.0L Flat-6 Twin-Turbo", 2981, 6, 385, 450, 4.2m, 293, 8.0m, 64, 4519, 1852, 1300, 2450, 120),
            ["porsche-cayenne"] = ("3.0L V6 Twin-Turbo", 2995, 6, 353, 500, 6.0m, 245, 9.0m, 90, 4926, 1983, 1696, 2895, 199),
            ["volvo-xc60-b5-awd"] = ("B420T2 I4 MHEV", 1969, 4, 250, 350, 6.9m, 220, 7.0m, 71, 4708, 1902, 1658, 2865, 216),
            ["volvo-s90-b5-awd"] = ("B420T2 I4 MHEV", 1969, 4, 250, 350, 7.2m, 230, 6.8m, 60, 4963, 1879, 1443, 2941, 152),
            ["ford-ranger-20-bi-turbo"] = ("Panther I4 Bi-Turbo Diesel", 1996, 4, 213, 500, 10.5m, 180, 7.6m, 80, 5370, 1918, 1884, 3270, 237),
            ["ford-everest-20-bi-turbo"] = ("Panther I4 Bi-Turbo Diesel", 1996, 4, 213, 500, 10.8m, 175, 7.8m, 80, 4904, 1918, 1842, 2915, 227),
            ["vinfast-vf8-eco"] = ("Dual Motor EV", null, null, 402, 620, 5.3m, 200, null, null, 4750, 1935, 1667, 2950, 179),
            ["hyundai-tucson-20-premium"] = ("Smartstream G2.0", 1999, 4, 156, 192, 10.1m, 185, 7.2m, 54, 4630, 1865, 1665, 2755, 181),
            ["kia-sportage-16t-gtline"] = ("Smartstream T-GDi", 1598, 4, 180, 265, 8.5m, 200, 6.8m, 54, 4660, 1865, 1660, 2755, 183),
            ["volkswagen-tiguan-20-tsi"] = ("EA888 TSI", 1984, 4, 180, 320, 8.3m, 210, 7.0m, 60, 4535, 1839, 1660, 2681, 189),
            ["nissan-x-trail-25-sv"] = ("QR25DE", 2488, 4, 181, 244, 9.2m, 190, 7.8m, 55, 4660, 1840, 1720, 2705, 212),
        };
        foreach (var v in variants)
        {
            if (!s.TryGetValue(v.Slug, out var sp)) continue;
            db.VehicleSpecifications.Add(new VehicleSpecificationEntity
            {
                VariantId = v.VariantId,
                EngineType = sp.engine,
                EngineDisplacement = sp.disp,
                CylinderCount = sp.cyl,
                Horsepower = sp.hp,
                Torque = sp.torque,
                Acceleration0To100 = sp.accel,
                TopSpeed = sp.top,
                FuelConsumptionCombined = sp.combined,
                FuelTankCapacity = sp.tank,
                LengthMm = sp.len,
                WidthMm = sp.w,
                HeightMm = sp.h,
                WheelbaseMm = sp.wb,
                GroundClearanceMm = sp.gc
            });
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedVariantColors(AutoHubDbContext db, List<VehicleVariantEntity> variants, Dictionary<string, ColorEntity> colors)
    {
        var cm = new Dictionary<string, string[]>
        {
            ["bmw-3-series-320i"] = new[] { "Alpine White", "Black Sapphire", "Mineral Grey", "Phytonic Blue" },
            ["bmw-3-series-330i-m-sport"] = new[] { "Alpine White", "Black Sapphire", "Portimao Blue", "Mineral Grey" },
            ["bmw-5-series-520i"] = new[] { "Alpine White", "Black Sapphire", "Mineral Grey", "Cavansite Blue" },
            ["bmw-x3-xdrive20d"] = new[] { "Alpine White", "Black Sapphire", "Phytonic Blue", "Mineral Grey" },
            ["bmw-x5-xdrive40i"] = new[] { "Alpine White", "Black Sapphire", "Manhattan Green", "Mineral Grey" },
            ["mercedes-c-class-c200"] = new[] { "Polar White", "Obsidian Black", "Selenite Grey", "Cavansite Blue" },
            ["mercedes-c-class-c300-amg"] = new[] { "Polar White", "Obsidian Black", "Selenite Grey", "Iridium Silver" },
            ["mercedes-e-class-e200"] = new[] { "Polar White", "Obsidian Black", "Selenite Grey", "Cavansite Blue" },
            ["mercedes-glc-200-4matic"] = new[] { "Polar White", "Obsidian Black", "Selenite Grey", "Iridium Silver" },
            ["audi-a4-45-tfsi"] = new[] { "Ibis White", "Mythos Black", "Navarra Blue", "Daytona Grey" },
            ["audi-a6-45-tfsi-quattro"] = new[] { "Ibis White", "Mythos Black", "Navarra Blue", "Daytona Grey" },
            ["audi-q5-45-tfsi-quattro"] = new[] { "Ibis White", "Mythos Black", "Navarra Blue", "Daytona Grey" },
            ["toyota-camry-25q"] = new[] { "Platinum White Pearl", "Attitude Black", "Sonic Titanium", "Graphite Grey" },
            ["toyota-corolla-cross-18v"] = new[] { "Platinum White Pearl", "Attitude Black", "Soul Red Crystal", "Phytonic Blue" },
            ["toyota-fortuner-24-vrz"] = new[] { "Platinum White Pearl", "Attitude Black", "Sonic Titanium", "Mineral Grey" },
            ["honda-civic-15rs"] = new[] { "Platinum White Pearl", "Attitude Black", "Machine Grey", "Soul Red Crystal" },
            ["honda-cr-v-15-l"] = new[] { "Platinum White Pearl", "Attitude Black", "Machine Grey" },
            ["mazda-cx5-25t"] = new[] { "Snowflake White Pearl", "Soul Red Crystal", "Jet Black", "Machine Grey" },
            ["mazda3-20-premium"] = new[] { "Snowflake White Pearl", "Soul Red Crystal", "Jet Black", "Machine Grey" },
            ["lexus-es-300h"] = new[] { "Eminent White Pearl", "Obsidian Black", "Sonic Titanium", "Graphite Grey" },
            ["lexus-rx-350h"] = new[] { "Snowflake White Pearl", "Obsidian Black", "Sonic Titanium", "Soul Red Crystal" },
            ["porsche-911-carrera"] = new[] { "Carrara White", "Jet Black", "Guards Red", "Racing Yellow" },
            ["porsche-cayenne"] = new[] { "Carrara White", "Jet Black", "Racing Yellow", "Moonlight Blue" },
            ["volvo-xc60-b5-awd"] = new[] { "Crystal White Pearl", "Onyx Black", "Thunder Grey", "Denim Blue" },
            ["volvo-s90-b5-awd"] = new[] { "Crystal White Pearl", "Onyx Black", "Thunder Grey", "Denim Blue" },
            ["ford-ranger-20-bi-turbo"] = new[] { "Snowflake White Pearl", "Absolute Black", "Ingot Silver", "True Blue" },
            ["ford-everest-20-bi-turbo"] = new[] { "Snowflake White Pearl", "Absolute Black", "Ingot Silver", "True Blue" },
            ["vinfast-vf8-eco"] = new[] { "Alpine White", "Obsidian Black", "Soul Red Crystal", "Phytonic Blue" },
            ["hyundai-tucson-20-premium"] = new[] { "Platinum White Pearl", "Attitude Black", "Sonic Titanium", "Graphite Grey" },
            ["kia-sportage-16t-gtline"] = new[] { "Platinum White Pearl", "Attitude Black", "Machine Grey", "Soul Red Crystal" },
            ["volkswagen-tiguan-20-tsi"] = new[] { "Ibis White", "Mythos Black", "Daytona Grey", "Navarra Blue" },
            ["nissan-x-trail-25-sv"] = new[] { "Platinum White Pearl", "Attitude Black", "Sonic Titanium", "Ingot Silver" },
        };
        foreach (var v in variants)
        {
            if (!cm.TryGetValue(v.Slug, out var names)) continue;
            int order = 0;
            foreach (var name in names)
            {
                if (!colors.TryGetValue(name, out var c)) continue;
                db.VariantColors.Add(new VariantColorEntity { VariantId = v.VariantId, ColorId = c.ColorId, SortOrder = order++, IsAvailable = true });
            }
        }
        await db.SaveChangesAsync();
    }

    private static async Task SeedMedia(AutoHubDbContext db, List<VehicleVariantEntity> variants)
    {
        var mm = new Dictionary<string, string>
        {
            ["bmw-3-series-320i"] = "/Images/BMW/3-Series/bmw-3-series-320i.jpg",
            ["bmw-3-series-330i-m-sport"] = "/Images/BMW/3-Series/bmw-3-series-320i.jpg",
            ["bmw-5-series-520i"] = "/Images/BMW/5-Series/bmw-5-series-520i.jpg",
            ["bmw-x3-xdrive20d"] = "/Images/BMW/X3/bmw-x3-xdrive20d.jpg",
            ["bmw-x5-xdrive40i"] = "/Images/BMW/X5/bmw-x5-40i.jpg",
            ["mercedes-c-class-c200"] = "/Images/Mercedes-Benz/C-Class/mercedes-c200.jpg",
            ["mercedes-c-class-c300-amg"] = "/Images/Mercedes-Benz/C-Class/mercedes-c200.jpg",
            ["mercedes-e-class-e200"] = "/Images/Mercedes-Benz/E-Class/mercedes-e300.jpg",
            ["mercedes-glc-200-4matic"] = "/Images/Mercedes-Benz/GLC/mercedes-glc-200.jpg",
            ["audi-a4-45-tfsi"] = "/Images/Audi/A4/audi-a4-45-tfsi.jpg",
            ["audi-a6-45-tfsi-quattro"] = "/Images/Audi/A6/audi-a6-45-tfsi-quattro.jpg",
            ["audi-q5-45-tfsi-quattro"] = "/Images/Audi/Q5/audi-q5-45-tfsi.jpg",
            ["toyota-camry-25q"] = "/Images/Toyota/Camry/toyota-camry-25q.jpg",
            ["toyota-corolla-cross-18v"] = "/Images/Toyota/Corolla-Cross/toyota-corolla-cross-18v.jpg",
            ["toyota-fortuner-24-vrz"] = "/Images/Toyota/Fortuner/toyota-fortuner-24-vrz.jpg",
            ["honda-civic-15rs"] = "/Images/Honda/Civic/honda-civic-15rs.jpg",
            ["honda-cr-v-15-l"] = "/Images/Honda/CR-V/honda-cr-v-15-l.jpg",
            ["mazda-cx5-25t"] = "/Images/Mazda/CX-5/mazda-cx5-25t.jpg",
            ["mazda3-20-premium"] = "/Images/Mazda/Mazda3/mazda3-20-premium.jpg",
            ["lexus-es-300h"] = "/Images/Lexus/ES/lexus-es-300h.jpg",
            ["lexus-rx-350h"] = "/Images/Lexus/RX/lexus-rx-350h.jpg",
            ["porsche-911-carrera"] = "/Images/Porsche/911/porsche-911-carrera.jpg",
            ["porsche-cayenne"] = "/Images/Porsche/Cayenne/porsche-cayenne.jpg",
            ["volvo-xc60-b5-awd"] = "/Images/Volvo/XC60/volvo-xc60-b5.jpg",
            ["volvo-s90-b5-awd"] = "/Images/Volvo/S90/volvo-s90-b5-awd.jpg",
            ["ford-ranger-20-bi-turbo"] = "/Images/Ford/Ranger/ford-ranger-20-bi-turbo.jpg",
            ["ford-everest-20-bi-turbo"] = "/Images/Ford/Everest/ford-everest-20-bi-turbo.jpg",
            ["vinfast-vf8-eco"] = "/Images/VinFast/VF8/vinfast-vf8-eco.svg",
            ["hyundai-tucson-20-premium"] = "/Images/Hyundai/Tucson/hyundai-tucson-20-premium.svg",
            ["kia-sportage-16t-gtline"] = "/Images/Kia/Sportage/kia-sportage-16t-gtline.svg",
            ["volkswagen-tiguan-20-tsi"] = "/Images/Volkswagen/Tiguan/volkswagen-tiguan-20-tsi.svg",
            ["nissan-x-trail-25-sv"] = "/Images/Nissan/X-Trail/nissan-x-trail-25-sv.svg",
        };

        var existingMediaVariants = await db.VariantMedia
            .Where(vm => vm.MediaRole == "HERO")
            .Select(vm => vm.VariantId)
            .ToListAsync();

        var toAdd = new List<(MediaAssetEntity media, int variantId)>();
        foreach (var v in variants)
        {
            if (existingMediaVariants.Contains(v.VariantId)) continue;
            if (!mm.TryGetValue(v.Slug, out var path)) continue;
            var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            var mime = ext == ".jpg" || ext == ".jpeg" ? "image/jpeg" : ext == ".png" ? "image/png" : "image/svg+xml";
            var media = new MediaAssetEntity { MediaType = "IMAGE", FileName = System.IO.Path.GetFileName(path), FilePath = path, MimeType = mime, AltText = $"{v.VariantName} hero image", Title = v.VariantName };
            db.MediaAssets.Add(media);
            toAdd.Add((media, v.VariantId));
        }
        await db.SaveChangesAsync();
        foreach (var (media, variantId) in toAdd)
        {
            db.VariantMedia.Add(new VariantMediaEntity { VariantId = variantId, MediaId = media.MediaId, MediaRole = "HERO", SortOrder = 0, IsPrimary = true });
        }
        await db.SaveChangesAsync();
    }
}
