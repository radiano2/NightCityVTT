using MongoDB.Driver;
using NightCityVTT.Models;
using NightCityVTT.Shared.Models;

namespace NightCityVTT.Services;

public class DatabaseSeeder
{
    private readonly IMongoDatabase _db;
    private readonly ILogger<DatabaseSeeder> _logger;

    private readonly IMongoCollection<Skill> _skills;
    private readonly IMongoCollection<Role> _roles;
    private readonly IMongoCollection<GearItem> _gear;

    public DatabaseSeeder(IMongoClient mongoClient, IConfiguration configuration, ILogger<DatabaseSeeder> logger)
    {
        var dbName = configuration["MongoDB:DatabaseName"] ?? "NightCityVTT";
        _db = mongoClient.GetDatabase(dbName);
        _skills = _db.GetCollection<Skill>("Skills");
        _roles = _db.GetCollection<Role>("Roles");
        _gear = _db.GetCollection<GearItem>("StartingGear");
        _logger = logger;
    }

    public async Task<SeedReport> SeedInitialData()
    {
        var report = new SeedReport();
        try
        {
            await SeedSkills();
            await SeedRoles();
            await SeedStartingGear();
            await SeedDefaultSettings();
            await SeedExampleStories();
            await SeedTestCharacters();

            report.SkillCount = (int)await _skills.CountDocumentsAsync(FilterDefinition<Skill>.Empty);
            report.RoleCount = (int)await _roles.CountDocumentsAsync(FilterDefinition<Role>.Empty);
            report.GearCount = (int)await _gear.CountDocumentsAsync(FilterDefinition<GearItem>.Empty);
            report.Success = true;
            report.Message = "Database seeded successfully with CP2020 core rulebook data and system defaults.";

            _logger.LogInformation("Database seeded: {Skills} skills, {Roles} roles, {Gear} gear items.",
                report.SkillCount, report.RoleCount, report.GearCount);
        }
        catch (Exception ex)
        {
            report.Success = false;
            report.Message = "Seeding failed.";
            report.Errors.Add(ex.Message);
            _logger.LogError(ex, "Database seeding failed.");
        }
        return report;
    }

    private async Task SeedDefaultSettings()
    {
        var settingsCollection = _db.GetCollection<SystemSettingsDocument>("Settings");
        var existing = await settingsCollection.Find(_ => true).FirstOrDefaultAsync();
        if (existing == null)
        {
            await settingsCollection.InsertOneAsync(new SystemSettingsDocument
            {
                Theme = "",
                AnimationIntervalMs = 5000,
                CrtFilterEnabled = false
            });
        }
    }

    private async Task SeedExampleStories()
    {
        var storiesCollection = _db.GetCollection<StoryDocument>("Stories");
        var count = await storiesCollection.CountDocumentsAsync(FilterDefinition<StoryDocument>.Empty);
        if (count > 0) return;

        var stories = new List<StoryDocument>
        {
            new StoryDocument
            {
                Title = "CORPORATE EXTRACTION // ARASAKA TOWER",
                AuthorId = "SYSTEM_DEFAULT",
                Stages = new List<StoryStageDto>
                {
                    new StoryStageDto { Title = "THE INFILTRATION", Description = "You are approaching the service entrance of Arasaka Tower via the abandoned subway tunnels. Security sensors are active, and the door lock is a high-end biometric scanner.", HasSkillCheck = true, SkillName = "Electronic Security", TargetDifficulty = 20 },
                    new StoryStageDto { Title = "THE EXTRACTION", Description = "You have bypassed the lock and entered the R&D sub-level. The target, Dr. Yamada, is currently being interrogated by a pair of Arasaka corporate solos. You must take them down quickly before they alert the rest of the facility.", HasSkillCheck = true, SkillName = "Stealth", TargetDifficulty = 15 },
                    new StoryStageDto { Title = "THE GETAWAY", Description = "With Yamada secured, alarms are blaring across the tower. An Arasaka AV-4 drops onto the loading bay, blocking your escape path. You need to commandeer it or find another way out under heavy fire.", HasSkillCheck = true, SkillName = "Pilot (Vectored Thrust)", TargetDifficulty = 25 }
                }
            },
            new StoryDocument
            {
                Title = "DATA HEIST // PETROCHEM SERVERS",
                AuthorId = "SYSTEM_DEFAULT",
                Stages = new List<StoryStageDto>
                {
                    new StoryStageDto { Title = "JACKING IN", Description = "You are jacked into a secluded terminal outside Petrochem's Night City headquarters. The initial Data Wall is formidable, guarded by a Hellhound Black ICE program.", HasSkillCheck = true, SkillName = "Interface", TargetDifficulty = 20 },
                    new StoryStageDto { Title = "DATA SEARCH", Description = "You've slipped past the ICE. You are now inside the main databank, searching for the encrypted files detailing their illegal CHOOH2 dumping sites. The file system is a labyrinth.", HasSkillCheck = true, SkillName = "System Knowledge", TargetDifficulty = 15 },
                    new StoryStageDto { Title = "LOGGING OUT", Description = "Files acquired. However, Petrochem Netrunners have detected your intrusion and are deploying a Brainwipe program. You need to disconnect immediately before your neural pathways are fried.", HasSkillCheck = true, SkillName = "Interface", TargetDifficulty = 25 }
                }
            },
            new StoryDocument
            {
                Title = "STREET NEGOTIATION // COMBAT ZONE",
                AuthorId = "SYSTEM_DEFAULT",
                Stages = new List<StoryStageDto>
                {
                    new StoryStageDto { Title = "THE MEET", Description = "You are meeting with a notorious Fixer, 'Sly' Malone, in a dimly lit bar deep within the Combat Zone. You need to buy information regarding a missing shipment of cyberware. The atmosphere is tense.", HasSkillCheck = true, SkillName = "Streetwise", TargetDifficulty = 15 },
                    new StoryStageDto { Title = "THE NEGOTIATION", Description = "Malone knows you are desperate. He is demanding an exorbitant price for the information. You need to talk him down without making him walk away from the table.", HasSkillCheck = true, SkillName = "Persuasion & Fast Talk", TargetDifficulty = 20 },
                    new StoryStageDto { Title = "THE AMBUSH", Description = "As the deal concludes, members of the 'Iron Sights' boostergang burst into the bar, guns drawn. They claim the cyberware belongs to them. You need to react quickly to survive the crossfire.", HasSkillCheck = true, SkillName = "Awareness/Notice", TargetDifficulty = 15 }
                }
            }
        };

        await storiesCollection.InsertManyAsync(stories);
    }

    public async Task<SeedReport> ValidateDatabase()
    {
        var report = new SeedReport();
        try
        {
            report.SkillCount = (int)await _skills.CountDocumentsAsync(FilterDefinition<Skill>.Empty);
            report.RoleCount = (int)await _roles.CountDocumentsAsync(FilterDefinition<Role>.Empty);
            report.GearCount = (int)await _gear.CountDocumentsAsync(FilterDefinition<GearItem>.Empty);

            report.CollectionCounts["Skills"] = report.SkillCount;
            report.CollectionCounts["Roles"] = report.RoleCount;
            report.CollectionCounts["StartingGear"] = report.GearCount;

            if (report.SkillCount == 0)
                report.ValidationWarnings.Add("Skills collection is empty — database has not been seeded.");
            if (report.RoleCount == 0)
                report.ValidationWarnings.Add("Roles collection is empty — database has not been seeded.");
            if (report.GearCount == 0)
                report.ValidationWarnings.Add("StartingGear collection is empty — database has not been seeded.");

            // Schema consistency: every skill must have a name and linkedAttribute
            var skillsMissingData = await _skills
                .Find(s => s.Name == string.Empty || s.LinkedAttribute == string.Empty)
                .CountDocumentsAsync();
            if (skillsMissingData > 0)
                report.ValidationWarnings.Add($"{skillsMissingData} skill(s) missing name or linkedAttribute.");

            // Every role must have a special ability
            var rolesMissingAbility = await _roles
                .Find(r => r.SpecialAbility == string.Empty)
                .CountDocumentsAsync();
            if (rolesMissingAbility > 0)
                report.ValidationWarnings.Add($"{rolesMissingAbility} role(s) missing specialAbility.");

            // Every gear item must have a category and a role assignment
            var gearMissingData = await _gear
                .Find(g => g.Category == string.Empty)
                .CountDocumentsAsync();
            if (gearMissingData > 0)
                report.ValidationWarnings.Add($"{gearMissingData} gear item(s) missing category.");

            report.Success = true;
            report.Message = report.ValidationWarnings.Count == 0
                ? "Database validation passed with no issues."
                : $"Database validation completed with {report.ValidationWarnings.Count} warning(s).";
        }
        catch (Exception ex)
        {
            report.Success = false;
            report.Message = "Validation failed.";
            report.Errors.Add(ex.Message);
            _logger.LogError(ex, "Database validation failed.");
        }
        return report;
    }

    private async Task SeedSkills()
    {
        await _skills.DeleteManyAsync(FilterDefinition<Skill>.Empty);
        await _skills.InsertManyAsync(GetSkills());
    }

    private async Task SeedRoles()
    {
        await _roles.DeleteManyAsync(FilterDefinition<Role>.Empty);
        await _roles.InsertManyAsync(GetRoles());
    }

    private async Task SeedStartingGear()
    {
        await _gear.DeleteManyAsync(FilterDefinition<GearItem>.Empty);
        await _gear.InsertManyAsync(GetStartingGear());
    }

    private static IEnumerable<Skill> GetSkills() =>
    [
        // INT-based skills
        new Skill { Name = "Awareness/Notice",           LinkedAttribute = "INT", Category = "General",   IpMultiplier = 1, Description = "Ability to observe details, spot ambushes, and notice concealed items." },
        new Skill { Name = "Biology",                    LinkedAttribute = "INT", Category = "Education",  IpMultiplier = 2, Description = "Knowledge of living organisms and life processes." },
        new Skill { Name = "Chemistry",                  LinkedAttribute = "INT", Category = "Education",  IpMultiplier = 2, Description = "Understanding of chemical compounds, reactions, and synthesis." },
        new Skill { Name = "Composition",                LinkedAttribute = "INT", Category = "Arts",       IpMultiplier = 2, Description = "Ability to write songs, stories, or other creative works." },
        new Skill { Name = "Diagnose Illness",           LinkedAttribute = "INT", Category = "Medical",    IpMultiplier = 2, Description = "Identify diseases, infections, and medical conditions." },
        new Skill { Name = "Education & General Knowledge", LinkedAttribute = "INT", Category = "Education", IpMultiplier = 1, Description = "General academic knowledge across multiple disciplines." },
        new Skill { Name = "Expert",                     LinkedAttribute = "INT", Category = "Education",  IpMultiplier = 2, Description = "Deep knowledge in a single specialized field (player defined)." },
        new Skill { Name = "Gamble",                     LinkedAttribute = "INT", Category = "General",    IpMultiplier = 1, Description = "Knowledge of gambling games, odds, and cheating techniques." },
        new Skill { Name = "Geology",                    LinkedAttribute = "INT", Category = "Education",  IpMultiplier = 2, Description = "Knowledge of rocks, minerals, and terrain types." },
        new Skill { Name = "Hide/Evade",                 LinkedAttribute = "INT", Category = "General",    IpMultiplier = 1, Description = "Lose a tail, hide from pursuers, and avoid detection." },
        new Skill { Name = "History",                    LinkedAttribute = "INT", Category = "Education",  IpMultiplier = 2, Description = "Knowledge of historical events up to the present day." },
        new Skill { Name = "Know Language",              LinkedAttribute = "INT", Category = "Language",   IpMultiplier = 2, Description = "Ability to read and write in a foreign language." },
        new Skill { Name = "Library Search",             LinkedAttribute = "INT", Category = "Education",  IpMultiplier = 1, Description = "Use databases, the Net, and libraries to find information." },
        new Skill { Name = "Mathematics",                LinkedAttribute = "INT", Category = "Education",  IpMultiplier = 2, Description = "Understanding of mathematical concepts and computation." },
        new Skill { Name = "Physics",                    LinkedAttribute = "INT", Category = "Education",  IpMultiplier = 2, Description = "Understanding of physical laws and applied mechanics." },
        new Skill { Name = "Programming",                LinkedAttribute = "INT", Category = "Technical",  IpMultiplier = 2, Description = "Write and modify software; essential for netrunners." },
        new Skill { Name = "Shadow/Track",               LinkedAttribute = "INT", Category = "General",    IpMultiplier = 1, Description = "Follow a target without being detected." },
        new Skill { Name = "Stock Market",               LinkedAttribute = "INT", Category = "Education",  IpMultiplier = 2, Description = "Understand and manipulate financial markets." },
        new Skill { Name = "System Knowledge",           LinkedAttribute = "INT", Category = "Technical",  IpMultiplier = 2, Description = "Knowledge of computer architectures and operating systems." },
        new Skill { Name = "Teaching",                   LinkedAttribute = "INT", Category = "Education",  IpMultiplier = 2, Description = "Ability to instruct others in skills you possess." },
        new Skill { Name = "Wilderness Survival",        LinkedAttribute = "INT", Category = "General",    IpMultiplier = 1, Description = "Find food, shelter, and navigate in the wilderness." },

        // REF-based skills
        new Skill { Name = "Archery",                    LinkedAttribute = "REF", Category = "Combat",     IpMultiplier = 2, Description = "Use of bows, crossbows, and other archery weapons." },
        new Skill { Name = "Athletics",                  LinkedAttribute = "REF", Category = "General",    IpMultiplier = 1, Description = "General athletic ability: running, jumping, climbing." },
        new Skill { Name = "Brawling",                   LinkedAttribute = "REF", Category = "Combat",     IpMultiplier = 1, Description = "Unarmed combat using punches, kicks, and takedowns." },
        new Skill { Name = "Dance",                      LinkedAttribute = "REF", Category = "Arts",       IpMultiplier = 2, Description = "Formal and informal dance styles." },
        new Skill { Name = "Dodge & Escape",             LinkedAttribute = "REF", Category = "Combat",     IpMultiplier = 1, Description = "Avoid attacks and escape grapples." },
        new Skill { Name = "Driving",                    LinkedAttribute = "REF", Category = "General",    IpMultiplier = 1, Description = "Operate ground vehicles: cars, trucks, APCs." },
        new Skill { Name = "Fencing",                    LinkedAttribute = "REF", Category = "Combat",     IpMultiplier = 2, Description = "Formal sword-fighting techniques." },
        new Skill { Name = "Handgun",                    LinkedAttribute = "REF", Category = "Combat",     IpMultiplier = 1, Description = "Use of pistols and revolvers." },
        new Skill { Name = "Heavy Weapons",              LinkedAttribute = "REF", Category = "Combat",     IpMultiplier = 2, Description = "Operate crew-served and heavy weapons: rocket launchers, miniguns." },
        new Skill { Name = "Martial Arts",               LinkedAttribute = "REF", Category = "Combat",     IpMultiplier = 3, Description = "Formal unarmed combat discipline (style chosen at creation)." },
        new Skill { Name = "Melee",                      LinkedAttribute = "REF", Category = "Combat",     IpMultiplier = 1, Description = "Use of knives, clubs, swords, and other hand-held weapons." },
        new Skill { Name = "Motorcycle",                 LinkedAttribute = "REF", Category = "General",    IpMultiplier = 1, Description = "Operate motorcycles and powered two-wheelers." },
        new Skill { Name = "Operate Heavy Machinery",   LinkedAttribute = "REF", Category = "Technical",  IpMultiplier = 2, Description = "Use construction equipment, industrial machines, and heavy vehicles." },
        new Skill { Name = "Pilot (Fixed Wing)",         LinkedAttribute = "REF", Category = "General",    IpMultiplier = 3, Description = "Fly fixed-wing aircraft: jets and propeller planes." },
        new Skill { Name = "Pilot (Gyro)",               LinkedAttribute = "REF", Category = "General",    IpMultiplier = 3, Description = "Fly rotary-wing aircraft: helicopters and gyrocraft." },
        new Skill { Name = "Pilot (Vectored Thrust)",    LinkedAttribute = "REF", Category = "General",    IpMultiplier = 3, Description = "Fly AV-style vectored thrust aircraft." },
        new Skill { Name = "Pilot (Dirigible)",          LinkedAttribute = "REF", Category = "General",    IpMultiplier = 3, Description = "Operate dirigibles and lighter-than-air craft." },
        new Skill { Name = "Rifle",                      LinkedAttribute = "REF", Category = "Combat",     IpMultiplier = 1, Description = "Use of rifles, shotguns, and long arms." },
        new Skill { Name = "Stealth",                    LinkedAttribute = "REF", Category = "General",    IpMultiplier = 1, Description = "Move quietly, conceal yourself, and avoid notice." },
        new Skill { Name = "Submachine Gun",             LinkedAttribute = "REF", Category = "Combat",     IpMultiplier = 1, Description = "Use of SMGs and automatic pistols in full-auto mode." },

        // TECH-based skills
        new Skill { Name = "AV Tech",                    LinkedAttribute = "TECH", Category = "Technical", IpMultiplier = 2, Description = "Repair and maintain AV and vectored-thrust vehicles." },
        new Skill { Name = "Basic Tech",                 LinkedAttribute = "TECH", Category = "Technical", IpMultiplier = 1, Description = "General mechanical and electronic repair." },
        new Skill { Name = "Cryotank Operation",         LinkedAttribute = "TECH", Category = "Medical",   IpMultiplier = 2, Description = "Operate cryogenic storage tanks for preserving critically injured." },
        new Skill { Name = "Cyberdeck Design",           LinkedAttribute = "TECH", Category = "Technical", IpMultiplier = 3, Description = "Design and build custom cyberdecks." },
        new Skill { Name = "Cybertech",                  LinkedAttribute = "TECH", Category = "Technical", IpMultiplier = 2, Description = "Install, remove, and repair cyberware." },
        new Skill { Name = "Demo & Explosives",          LinkedAttribute = "TECH", Category = "Combat",    IpMultiplier = 2, Description = "Set, disarm, and detonate explosive devices." },
        new Skill { Name = "Electronics",                LinkedAttribute = "TECH", Category = "Technical", IpMultiplier = 2, Description = "Build and repair electronic devices and circuits." },
        new Skill { Name = "Electronic Security",        LinkedAttribute = "TECH", Category = "Technical", IpMultiplier = 2, Description = "Bypass alarms, cameras, and electronic locks." },
        new Skill { Name = "First Aid",                  LinkedAttribute = "TECH", Category = "Medical",   IpMultiplier = 1, Description = "Stabilize wounds, stop bleeding, and provide emergency care." },
        new Skill { Name = "Forgery",                    LinkedAttribute = "TECH", Category = "General",   IpMultiplier = 2, Description = "Create convincing fake documents, currency, and IDs." },
        new Skill { Name = "Gyrotech",                   LinkedAttribute = "TECH", Category = "Technical", IpMultiplier = 2, Description = "Repair and maintain gyrocraft and rotor systems." },
        new Skill { Name = "Paint or Draw",              LinkedAttribute = "TECH", Category = "Arts",      IpMultiplier = 2, Description = "Create visual art through painting and drawing." },
        new Skill { Name = "Photo & Film",               LinkedAttribute = "TECH", Category = "Arts",      IpMultiplier = 2, Description = "Operate cameras and edit photo/video media." },
        new Skill { Name = "Pick Lock",                  LinkedAttribute = "TECH", Category = "General",   IpMultiplier = 1, Description = "Open mechanical locks without the correct key." },
        new Skill { Name = "Pick Pocket",                LinkedAttribute = "TECH", Category = "General",   IpMultiplier = 1, Description = "Steal items from a person without being detected." },
        new Skill { Name = "Play Instrument",            LinkedAttribute = "TECH", Category = "Arts",      IpMultiplier = 2, Description = "Play a musical instrument with skill." },
        new Skill { Name = "Weaponsmith",                LinkedAttribute = "TECH", Category = "Technical", IpMultiplier = 2, Description = "Fabricate, repair, and modify firearms and melee weapons." },

        // COOL-based skills
        new Skill { Name = "Interrogation",              LinkedAttribute = "COOL", Category = "General",   IpMultiplier = 1, Description = "Extract information from unwilling subjects through psychological pressure." },
        new Skill { Name = "Intimidate",                 LinkedAttribute = "COOL", Category = "General",   IpMultiplier = 1, Description = "Frighten others into compliance through physical or psychological threat." },
        new Skill { Name = "Oratory",                    LinkedAttribute = "COOL", Category = "Social",    IpMultiplier = 2, Description = "Inspire and move crowds through public speaking." },
        new Skill { Name = "Personal Grooming",          LinkedAttribute = "COOL", Category = "Social",    IpMultiplier = 1, Description = "Maintain physical appearance and hygiene for social situations." },
        new Skill { Name = "Persuasion & Fast Talk",     LinkedAttribute = "COOL", Category = "Social",    IpMultiplier = 1, Description = "Convince others to do what you want through logical or rapid-fire argument." },
        new Skill { Name = "Resist Torture/Drugs",       LinkedAttribute = "COOL", Category = "General",   IpMultiplier = 2, Description = "Withstand physical and chemical interrogation." },
        new Skill { Name = "Streetwise",                 LinkedAttribute = "COOL", Category = "General",   IpMultiplier = 1, Description = "Navigate the criminal underworld, find contacts, and read the streets." },
        new Skill { Name = "Wardrobe & Style",           LinkedAttribute = "COOL", Category = "Social",    IpMultiplier = 1, Description = "Dress to impress for any social occasion." },

        // BODY-based skills
        new Skill { Name = "Endurance",                  LinkedAttribute = "BODY", Category = "General",   IpMultiplier = 1, Description = "Maintain peak performance through extended physical hardship." },
        new Skill { Name = "Strength Feat",              LinkedAttribute = "BODY", Category = "General",   IpMultiplier = 2, Description = "Perform feats of extraordinary strength: bend bars, lift vehicles." },
        new Skill { Name = "Swimming",                   LinkedAttribute = "BODY", Category = "General",   IpMultiplier = 1, Description = "Swim in any conditions." },

        // EMP-based skills
        new Skill { Name = "Human Perception",           LinkedAttribute = "EMP",  Category = "Social",    IpMultiplier = 2, Description = "Detect lies, sense emotions, and read intent." },
        new Skill { Name = "Interview",                  LinkedAttribute = "EMP",  Category = "Social",    IpMultiplier = 2, Description = "Draw out information through skillful questioning." },
        new Skill { Name = "Leadership",                 LinkedAttribute = "EMP",  Category = "Social",    IpMultiplier = 2, Description = "Inspire and command a group under pressure." },
        new Skill { Name = "Seduction",                  LinkedAttribute = "EMP",  Category = "Social",    IpMultiplier = 2, Description = "Attract and influence others through romantic appeal." },
        new Skill { Name = "Social",                     LinkedAttribute = "EMP",  Category = "Social",    IpMultiplier = 1, Description = "General social interactions: etiquette, networking, schmoozing." },

        // Special role abilities (treated as skills at level 7 by role)
        new Skill { Name = "Combat Sense",               LinkedAttribute = "REF",  Category = "Special",   IpMultiplier = 3, Description = "[Solo] Sense danger before it strikes; adds to Initiative and awareness rolls." },
        new Skill { Name = "Authority",                  LinkedAttribute = "COOL", Category = "Special",   IpMultiplier = 3, Description = "[Cop] Power of arrest and command; can commandeer resources within jurisdiction." },
        new Skill { Name = "Charismatic Leadership",     LinkedAttribute = "EMP",  Category = "Special",   IpMultiplier = 3, Description = "[Rockerboy] Lead a crowd or band; inspire mass action through performance." },
        new Skill { Name = "Credibility",                LinkedAttribute = "INT",  Category = "Special",   IpMultiplier = 3, Description = "[Media] Your word is trusted; can publish and broadcast with authority." },
        new Skill { Name = "Family",                     LinkedAttribute = "INT",  Category = "Special",   IpMultiplier = 3, Description = "[Nomad] Call on pack resources, safe houses, and Nomad contacts." },
        new Skill { Name = "Interface",                  LinkedAttribute = "INT",  Category = "Special",   IpMultiplier = 3, Description = "[Netrunner] Access, control, and attack systems through the Net." },
        new Skill { Name = "Jury Rig",                   LinkedAttribute = "TECH", Category = "Special",   IpMultiplier = 3, Description = "[Tech] Improvise repairs and create devices from available parts." },
        new Skill { Name = "Medical Tech",               LinkedAttribute = "TECH", Category = "Special",   IpMultiplier = 3, Description = "[Medtech] Perform surgery, install cyberware, and treat serious trauma." },
        new Skill { Name = "Resources",                  LinkedAttribute = "INT",  Category = "Special",   IpMultiplier = 3, Description = "[Corporate] Access corporate funding, personnel, and infrastructure." },
        new Skill { Name = "Streetdeal",                 LinkedAttribute = "INT",  Category = "Special",   IpMultiplier = 3, Description = "[Fixer] Connect buyers with sellers; know who has what and what it costs." },
    ];

    private static IEnumerable<Role> GetRoles() =>
    [
        new Role
        {
            Name = "Rockerboy",
            SpecialAbility = "Charismatic Leadership",
            SpecialAbilityDescription = "At level 7+ can lead and inspire mobs; at 3+ can influence individuals through performance.",
            Description = "Musicians and artists who use their talents to rebel against authority and inspire the masses in Night City.",
            PrimarySkills = ["Charismatic Leadership", "Persuasion & Fast Talk", "Composition"],
            CareerSkills  = ["Charismatic Leadership", "Composition", "Brawling", "Wardrobe & Style", "Streetwise", "Persuasion & Fast Talk", "Seduction", "Athletics", "Play Instrument", "Oratory"]
        },
        new Role
        {
            Name = "Solo",
            SpecialAbility = "Combat Sense",
            SpecialAbilityDescription = "Adds level to Awareness and Initiative; allows noticing ambushes and traps automatically.",
            Description = "Elite combat mercenaries and bodyguards — the best fighters money can buy in the dark future.",
            PrimarySkills = ["Combat Sense", "Handgun", "Rifle"],
            CareerSkills  = ["Combat Sense", "Handgun", "Rifle", "Submachine Gun", "Brawling", "Melee", "Dodge & Escape", "Athletics", "Awareness/Notice", "Stealth"]
        },
        new Role
        {
            Name = "Netrunner",
            SpecialAbility = "Interface",
            SpecialAbilityDescription = "Allows jacking into the Net to access, modify, and attack systems; level determines program space and Net combat ability.",
            Description = "Console cowboys and cyberhackers who surf the Net and crack corporate ICE for fun, profit, and principle.",
            PrimarySkills = ["Interface", "Electronics", "System Knowledge"],
            CareerSkills  = ["Interface", "Electronics", "Electronic Security", "System Knowledge", "Basic Tech", "Cryptography", "Programming", "Awareness/Notice", "Cyberdeck Design", "Hide/Evade"]
        },
        new Role
        {
            Name = "Tech",
            SpecialAbility = "Jury Rig",
            SpecialAbilityDescription = "Improvise functional repairs from scrap; level determines complexity of improvised devices.",
            Description = "Mechanics, engineers, and inventors who keep Night City's machines running — and build the weapons to take them apart.",
            PrimarySkills = ["Jury Rig", "Basic Tech", "Cybertech"],
            CareerSkills  = ["Jury Rig", "Basic Tech", "Electronics", "Cybertech", "Weaponsmith", "AV Tech", "Gyrotech", "Demo & Explosives", "First Aid", "Operate Heavy Machinery"]
        },
        new Role
        {
            Name = "Medtech",
            SpecialAbility = "Medical Tech",
            SpecialAbilityDescription = "Perform surgery, stabilize massive trauma, and install cyberware; level determines maximum wound severity treatable.",
            Description = "Combat surgeons and cyberware installers who keep bodies functional in a world where violence is currency.",
            PrimarySkills = ["Medical Tech", "First Aid", "Diagnose Illness"],
            CareerSkills  = ["Medical Tech", "First Aid", "Diagnose Illness", "Chemistry", "Cryotank Operation", "Education & General Knowledge", "Cybertech", "Basic Tech", "Awareness/Notice", "Human Perception"]
        },
        new Role
        {
            Name = "Media",
            SpecialAbility = "Credibility",
            SpecialAbilityDescription = "Your word carries weight; level determines how powerful the corporate entity that fears your broadcast is.",
            Description = "Journalists, vidcasters, and investigators who wield the power of information as a weapon against the corps.",
            PrimarySkills = ["Credibility", "Persuasion & Fast Talk", "Awareness/Notice"],
            CareerSkills  = ["Credibility", "Persuasion & Fast Talk", "Human Perception", "Education & General Knowledge", "Social", "Streetwise", "Awareness/Notice", "Interview", "Composition", "Photo & Film"]
        },
        new Role
        {
            Name = "Cop",
            SpecialAbility = "Authority",
            SpecialAbilityDescription = "Carry warrants and commandeer civilian resources; level determines the corporate/government tier subject to authority.",
            Description = "Law enforcement officers from City Cops to corporate Trauma Team — the thin blue line in a city of chrome and neon.",
            PrimarySkills = ["Authority", "Handgun", "Interrogation"],
            CareerSkills  = ["Authority", "Handgun", "Interrogation", "Intimidate", "Awareness/Notice", "Human Perception", "Stealth", "Shadow/Track", "Athletics", "Education & General Knowledge"]
        },
        new Role
        {
            Name = "Corporate",
            SpecialAbility = "Resources",
            SpecialAbilityDescription = "Access corporate money, personnel, and infrastructure; level determines resource tier available.",
            Description = "Executives, spies, and operatives who have climbed high enough up the corporate ladder to have power — and enough enemies to need it.",
            PrimarySkills = ["Resources", "Social", "Human Perception"],
            CareerSkills  = ["Resources", "Human Perception", "Education & General Knowledge", "Library Search", "Social", "Persuasion & Fast Talk", "Wardrobe & Style", "Personal Grooming", "Awareness/Notice", "Intimidate"]
        },
        new Role
        {
            Name = "Fixer",
            SpecialAbility = "Streetdeal",
            SpecialAbilityDescription = "Know who has what; level determines how rare the item or contact that can be located.",
            Description = "Black market brokers and information dealers who connect people with what they need — for a price.",
            PrimarySkills = ["Streetdeal", "Streetwise", "Human Perception"],
            CareerSkills  = ["Streetdeal", "Human Perception", "Streetwise", "Social", "Persuasion & Fast Talk", "Forgery", "Awareness/Notice", "Pick Pocket", "Hide/Evade", "Shadow/Track"]
        },
        new Role
        {
            Name = "Nomad",
            SpecialAbility = "Family",
            SpecialAbilityDescription = "Call on pack resources: vehicles, safehouses, and clan contacts; level determines pack size and resource depth.",
            Description = "Tribal wanderers of the Badlands who travel in massive family packs, living outside the megacities and surviving on the road.",
            PrimarySkills = ["Family", "Driving", "Wilderness Survival"],
            CareerSkills  = ["Family", "Driving", "Motorcycle", "Athletics", "Wilderness Survival", "Basic Tech", "Endurance", "Rifle", "Brawling", "First Aid"]
        },
    ];

    private static IEnumerable<GearItem> GetStartingGear() =>
    [
        // Rockerboy gear
        new GearItem { Name = "Microphone (Broadcast-Quality)", Category = "Equipment",  ForRole = "Rockerboy", Description = "High-end broadcast mic for concerts and guerrilla broadcasts.", CostEurobucks = 200,  Concealability = "N/A", Availability = "Common", WeightKg = 1.0 },
        new GearItem { Name = "Electric Guitar (Custom)",       Category = "Instrument",  ForRole = "Rockerboy", Description = "Custom shred guitar with built-in amp jack.",               CostEurobucks = 500,  Concealability = "N/A", Availability = "Common", WeightKg = 1.0 },
        new GearItem { Name = "Light Armorjack (SP14)",         Category = "Armor",       ForRole = "Rockerboy", Description = "Fashionable armored jacket — SP14.",                        CostEurobucks = 150,  Concealability = "J",   Availability = "Common", WeightKg = 2.0 },
        new GearItem { Name = "Medscanner",                     Category = "Medical",     ForRole = "Rockerboy", Description = "Handheld medical scanner for quick diagnostics.",            CostEurobucks = 300,  Concealability = "P",   Availability = "Common", WeightKg = 0.5 },

        // Solo gear
        new GearItem { Name = "Medium Pistol (9mm)",            Category = "Weapon",      ForRole = "Solo",      Description = "Standard 9mm pistol. 2d6+1 damage, 15-round magazine.",    CostEurobucks = 250,  Concealability = "P",   Availability = "Common", WeightKg = 1.5 },
        new GearItem { Name = "Body Armor (SP20)",              Category = "Armor",       ForRole = "Solo",      Description = "Military-grade body armor — SP20.",                         CostEurobucks = 700,  Concealability = "N/A", Availability = "Rare"  , WeightKg = 2.0 },
        new GearItem { Name = "Combat Knife",                   Category = "Weapon",      ForRole = "Solo",      Description = "High-carbon steel fighting knife. 1d6+2 damage.",          CostEurobucks = 30,   Concealability = "P",   Availability = "Common", WeightKg = 1.0 },
        new GearItem { Name = "Medscanner",                     Category = "Medical",     ForRole = "Solo",      Description = "Handheld medical scanner for quick diagnostics.",            CostEurobucks = 300,  Concealability = "P",   Availability = "Common", WeightKg = 0.5 },

        // Netrunner gear
        new GearItem { Name = "Cyberdeck (Standard)",           Category = "Cyberware",   ForRole = "Netrunner", Description = "Standard cyberdeck; 30 MU program space, 5 options.",       CostEurobucks = 1000, Concealability = "J",   Availability = "Rare"  , WeightKg = 1.0, HumanityCostDice = "2d6" },
        new GearItem { Name = "Neural Interface (Basic)",       Category = "Cyberware",   ForRole = "Netrunner", Description = "Basic neural link allowing deck connection.",                CostEurobucks = 500,  Concealability = "N/A", Availability = "Common", WeightKg = 1.0, HumanityCostDice = "2d6" },
        new GearItem { Name = "Light Armorjack (SP14)",         Category = "Armor",       ForRole = "Netrunner", Description = "Light armored jacket — SP14.",                              CostEurobucks = 150,  Concealability = "J",   Availability = "Common", WeightKg = 2.0 },
        new GearItem { Name = "Pocket Computer",                Category = "Equipment",   ForRole = "Netrunner", Description = "Powerful handheld computer for offline work.",               CostEurobucks = 200,  Concealability = "P",   Availability = "Common", WeightKg = 1.0 },

        // Tech gear
        new GearItem { Name = "Basic Tool Kit",                 Category = "Equipment",   ForRole = "Tech",      Description = "Complete set of hand tools for mechanical and electronic work.", CostEurobucks = 200, Concealability = "N/A", Availability = "Common", WeightKg = 5.0 },
        new GearItem { Name = "Electronic Multi-Meter",         Category = "Equipment",   ForRole = "Tech",      Description = "Diagnostic tool for circuits and power systems.",            CostEurobucks = 100,  Concealability = "P",   Availability = "Common", WeightKg = 1.0 },
        new GearItem { Name = "Light Armorjack (SP14)",         Category = "Armor",       ForRole = "Tech",      Description = "Work-worn armored jacket — SP14.",                          CostEurobucks = 150,  Concealability = "J",   Availability = "Common", WeightKg = 2.0 },
        new GearItem { Name = "Medscanner",                     Category = "Medical",     ForRole = "Tech",      Description = "Handheld medical scanner.",                                  CostEurobucks = 300,  Concealability = "P",   Availability = "Common", WeightKg = 0.5 },

        // Medtech gear
        new GearItem { Name = "Medscanner (Advanced)",          Category = "Medical",     ForRole = "Medtech",   Description = "Advanced model with full diagnostic suite.",                 CostEurobucks = 500,  Concealability = "J",   Availability = "Rare"  , WeightKg = 0.5 },
        new GearItem { Name = "Trauma Team Card (3 Uses)",      Category = "Medical",     ForRole = "Medtech",   Description = "Prepaid Trauma Team extraction — 3 uses.",                  CostEurobucks = 1000, Concealability = "P",   Availability = "Common", WeightKg = 0.5 },
        new GearItem { Name = "Pharmaceuticals (Stimpak x5)",   Category = "Medical",     ForRole = "Medtech",   Description = "Stimulant injectors; restore 1 BODY for 1 hour.",            CostEurobucks = 250,  Concealability = "P",   Availability = "Common", WeightKg = 0.5 },
        new GearItem { Name = "Light Armorjack (SP14)",         Category = "Armor",       ForRole = "Medtech",   Description = "Medical staff armored jacket — SP14.",                      CostEurobucks = 150,  Concealability = "J",   Availability = "Common", WeightKg = 2.0 },

        // Media gear
        new GearItem { Name = "Broadcast Camera (Shoulder-Mount)", Category = "Equipment", ForRole = "Media",    Description = "Professional broadcast camera with uplink capability.",       CostEurobucks = 800,  Concealability = "N/A", Availability = "Common", WeightKg = 1.0 },
        new GearItem { Name = "Digital Recorder",               Category = "Equipment",   ForRole = "Media",     Description = "High-quality audio/video recorder.",                         CostEurobucks = 150,  Concealability = "J",   Availability = "Common", WeightKg = 1.0 },
        new GearItem { Name = "Press Pass (Authentic)",         Category = "Document",    ForRole = "Media",     Description = "Legitimate press credentials; opens many doors.",            CostEurobucks = 100,  Concealability = "P",   Availability = "Rare"  , WeightKg = 1.0 },
        new GearItem { Name = "Light Armorjack (SP14)",         Category = "Armor",       ForRole = "Media",     Description = "Discreet armored jacket — SP14.",                           CostEurobucks = 150,  Concealability = "J",   Availability = "Common", WeightKg = 2.0 },

        // Cop gear
        new GearItem { Name = "Medium Pistol (Lawforce Issue)", Category = "Weapon",      ForRole = "Cop",       Description = "Standard issue sidearm. 2d6+1 damage, 15-round magazine.",  CostEurobucks = 250,  Concealability = "P",   Availability = "Common", WeightKg = 1.5 },
        new GearItem { Name = "Handcuffs (Smart)",              Category = "Equipment",   ForRole = "Cop",       Description = "Electronic handcuffs with biometric lock.",                  CostEurobucks = 50,   Concealability = "J",   Availability = "Common", WeightKg = 1.0 },
        new GearItem { Name = "Badge & ID",                     Category = "Document",    ForRole = "Cop",       Description = "Official badge granting arrest authority.",                  CostEurobucks = 0,    Concealability = "J",   Availability = "N/A"   , WeightKg = 1.0 },
        new GearItem { Name = "Medium Armor (SP18)",            Category = "Armor",       ForRole = "Cop",       Description = "Police-issue body armor — SP18.",                           CostEurobucks = 500,  Concealability = "N/A", Availability = "Common", WeightKg = 2.0 },

        // Corporate gear
        new GearItem { Name = "Corporate SmartCard (1000 eb)", Category = "Finance",     ForRole = "Corporate", Description = "Corporate expense account card, loaded with 1000 eurobucks.", CostEurobucks = 1000, Concealability = "P",   Availability = "N/A"   , WeightKg = 1.0 },
        new GearItem { Name = "Business Suit (Armored)",        Category = "Armor",       ForRole = "Corporate", Description = "Tailored armored suit — SP10, looks completely civilian.",   CostEurobucks = 600,  Concealability = "J",   Availability = "Rare"  , WeightKg = 2.0 },
        new GearItem { Name = "Personal Computer (High-End)",   Category = "Equipment",   ForRole = "Corporate", Description = "Encrypted laptop with corporate network access.",             CostEurobucks = 500,  Concealability = "J",   Availability = "Common", WeightKg = 1.0 },
        new GearItem { Name = "Pocket Secretary",               Category = "Equipment",   ForRole = "Corporate", Description = "Personal organizer, contacts database, and scheduler.",      CostEurobucks = 100,  Concealability = "P",   Availability = "Common", WeightKg = 1.0 },

        // Fixer gear
        new GearItem { Name = "Communications Scrambler",       Category = "Equipment",   ForRole = "Fixer",     Description = "Encrypts voice and data transmissions.",                     CostEurobucks = 300,  Concealability = "J",   Availability = "Rare"  , WeightKg = 1.0 },
        new GearItem { Name = "Pocket Secretary",               Category = "Equipment",   ForRole = "Fixer",     Description = "Encrypted contacts and deal database.",                      CostEurobucks = 100,  Concealability = "P",   Availability = "Common", WeightKg = 1.0 },
        new GearItem { Name = "Light Armorjack (SP14)",         Category = "Armor",       ForRole = "Fixer",     Description = "Stylish armored jacket — SP14.",                            CostEurobucks = 150,  Concealability = "J",   Availability = "Common", WeightKg = 2.0 },
        new GearItem { Name = "Hold-Out Pistol",                Category = "Weapon",      ForRole = "Fixer",     Description = "Easily concealed light pistol. 2d6 damage.",                CostEurobucks = 200,  Concealability = "P",   Availability = "Common", WeightKg = 1.5 },

        // Nomad gear
        new GearItem { Name = "Motorcycle (Pack-Issue)",        Category = "Vehicle",     ForRole = "Nomad",     Description = "Rugged off-road motorcycle. Pack markings on tank.",         CostEurobucks = 2000, Concealability = "N/A", Availability = "Rare"  , WeightKg = 500.0 },
        new GearItem { Name = "Light Armorjack (SP14)",         Category = "Armor",       ForRole = "Nomad",     Description = "Road-worn armored jacket — SP14.",                          CostEurobucks = 150,  Concealability = "J",   Availability = "Common", WeightKg = 2.0 },
        new GearItem { Name = "Hunting Rifle",                  Category = "Weapon",      ForRole = "Nomad",     Description = "Bolt-action rifle. 5d6 damage, excellent range.",           CostEurobucks = 400,  Concealability = "N/A", Availability = "Common", WeightKg = 4.5 },
        new GearItem { Name = "Basic Tool Kit",                 Category = "Equipment",   ForRole = "Nomad",     Description = "Essential tools for road repairs and field maintenance.",    CostEurobucks = 200,  Concealability = "N/A", Availability = "Common", WeightKg = 5.0 },
        new GearItem { Name = "Eclipse Arms Specter", Category = "Weapon", Description = "Smartlinked 9mm with suppressor ports – netrunner favorite.", CostEurobucks = 650, Concealability = "P", Availability = "C", Acc = "+1", Damage = "1D6+1", Range = "50m", Capacity = "15", ROF = "3", WeightKg = 3.0 },
        new GearItem { Name = "Viper Dynamics Street Judge", Category = "Weapon", Description = "Brutal .45 ACP for boostergang muscle.", CostEurobucks = 450, Concealability = "J", Availability = "C", Acc = "0", Damage = "2D6+1", Range = "75m", Capacity = "10", ROF = "2", WeightKg = 3.0 },
        new GearItem { Name = "Kang Tao Red Dragon", Category = "Weapon", Description = "Burst-fire pocket rocket with folding stock.", CostEurobucks = 780, Concealability = "J", Availability = "U", Acc = "0", Damage = "1D6", Range = "100m", Capacity = "32", ROF = "10", WeightKg = 3.0 },
        new GearItem { Name = "Militech Avenger", Category = "Weapon", Description = "Standard 5.56mm with optional grenade launcher.", CostEurobucks = 1250, Concealability = "N", Availability = "R", Acc = "+1", Damage = "5D6", Range = "400m", Capacity = "30", ROF = "10", WeightKg = 3.0 },
        new GearItem { Name = "NightEdge Ghost Blade", Category = "Weapon", Description = "Mono-edged smart-grip katana for samurai.", CostEurobucks = 1200, Concealability = "N", Availability = "R", Acc = "+1", Damage = "2D6+3", Range = "Melee", Capacity = "-", ROF = "-", WeightKg = 1.0 },
        new GearItem { Name = "Thunderfist Shock Knuckles", Category = "Weapon", Description = "Brass knuckles with built-in stun capacitors.", CostEurobucks = 380, Concealability = "P", Availability = "C", Acc = "0", Damage = "1D6+2", Range = "Melee", Capacity = "-", ROF = "-", WeightKg = 1.0 },
        new GearItem { Name = "Arasaka Wraith", Category = "Weapon", Description = "Suppressed .338 with thermal smart-scope.", CostEurobucks = 2100, Concealability = "N", Availability = "R", Acc = "+2", Damage = "6D6", Range = "800m", Capacity = "5", ROF = "1", WeightKg = 3.0 },
        new GearItem { Name = "Roadblock 12ga", Category = "Weapon", Description = "Pump-action flechette monster.", CostEurobucks = 520, Concealability = "L", Availability = "C", Acc = "0", Damage = "4D6", Range = "25m", Capacity = "8", ROF = "2", WeightKg = 3.0 },
        new GearItem { Name = "Militech Hellfire", Category = "Weapon", Description = "HE/flashbang launcher for clearing rooms.", CostEurobucks = 1850, Concealability = "N", Availability = "R", Acc = "-1", Damage = "Varies", Range = "150m", Capacity = "6", ROF = "1", WeightKg = 3.0 },
        new GearItem { Name = "Kang Tao Lightning", Category = "Weapon", Description = "Silent electromagnetic dart gun.", CostEurobucks = 2450, Concealability = "J", Availability = "U", Acc = "+1", Damage = "3D6+2", Range = "60m", Capacity = "8", ROF = "2", WeightKg = 3.0 },
        new GearItem { Name = "Arasaka Black Widow", Category = "Weapon", Description = "Compact smart-pistol with toxin dart option.", CostEurobucks = 950, Concealability = "P", Availability = "R", Acc = "+1", Damage = "2D6", Range = "40m", Capacity = "12", ROF = "3", WeightKg = 3.0 },
        new GearItem { Name = "Militech Bulldog", Category = "Weapon", Description = ".50 cal beast that kicks like a mule.", CostEurobucks = 680, Concealability = "J", Availability = "C", Acc = "0", Damage = "3D6", Range = "80m", Capacity = "8", ROF = "2", WeightKg = 3.0 },
        new GearItem { Name = "Nomad Sawtooth", Category = "Weapon", Description = "Rugged open-bolt SMG for badlands runs.", CostEurobucks = 920, Concealability = "L", Availability = "U", Acc = "0", Damage = "2D6", Range = "150m", Capacity = "40", ROF = "15", WeightKg = 3.0 },
        new GearItem { Name = "Trauma Team Reaper", Category = "Weapon", Description = "Medical-corp carbine with underbarrel med-injector.", CostEurobucks = 1680, Concealability = "N", Availability = "R", Acc = "+1", Damage = "4D6+2", Range = "350m", Capacity = "25", ROF = "8", WeightKg = 3.0 },
        new GearItem { Name = "Street Ronin Tanto", Category = "Weapon", Description = "Mono-tanto with hidden garrote wire.", CostEurobucks = 220, Concealability = "P", Availability = "C", Acc = "+2", Damage = "1D6+2", Range = "Melee", Capacity = "-", ROF = "-", WeightKg = 1.0 },
        new GearItem { Name = "Kang Tao Firestorm", Category = "Weapon", Description = "Backpack flamer for clearing alleys.", CostEurobucks = 3100, Concealability = "N", Availability = "R", Acc = "-2", Damage = "4D6 fire", Range = "10m", Capacity = "6 shots", ROF = "1", WeightKg = 3.0 },
        new GearItem { Name = "Arasaka Oni", Category = "Weapon", Description = "Tripod or vehicle-mounted shredder.", CostEurobucks = 4200, Concealability = "N", Availability = "R", Acc = "0", Damage = "7D6", Range = "500m", Capacity = "200", ROF = "20", WeightKg = 3.0 },
        new GearItem { Name = "Viper Venom", Category = "Weapon", Description = "Silent injector pistol for covert takedowns.", CostEurobucks = 890, Concealability = "P", Availability = "U", Acc = "+1", Damage = "1D6+drug", Range = "30m", Capacity = "20", ROF = "4", WeightKg = 3.0 },
        new GearItem { Name = "Militech Cyclone", Category = "Weapon", Description = "Gatling-style anti-personnel nightmare.", CostEurobucks = 6800, Concealability = "N", Availability = "R", Acc = "-1", Damage = "6D6", Range = "300m", Capacity = "500", ROF = "30", WeightKg = 3.0 },
        new GearItem { Name = "NightEdge Razor Whip", Category = "Weapon", Description = "Mono-wire whip that slices limbs clean off.", CostEurobucks = 750, Concealability = "J", Availability = "U", Acc = "0", Damage = "3D6", Range = "4m", Capacity = "-", ROF = "-", WeightKg = 3.0 },
        new GearItem { Name = "Kang Tao Dragonbreath", Category = "Weapon", Description = "Dragon-breath incendiary rounds.", CostEurobucks = 1450, Concealability = "L", Availability = "R", Acc = "-1", Damage = "5D6", Range = "20m", Capacity = "5", ROF = "1", WeightKg = 3.0 },
        new GearItem { Name = "Arasaka Shadow", Category = "Weapon", Description = "Ghost-suit integrated stealth sniper.", CostEurobucks = 3800, Concealability = "N", Availability = "R", Acc = "+3", Damage = "5D6+1", Range = "1000m", Capacity = "10", ROF = "1", WeightKg = 3.0 },
        new GearItem { Name = "Militech Hammer", Category = "Weapon", Description = "Disposable anti-vehicle rocket.", CostEurobucks = 5200, Concealability = "N", Availability = "R", Acc = "0", Damage = "8D6 AP", Range = "300m", Capacity = "1", ROF = "1", WeightKg = 3.0 },
        new GearItem { Name = "Street Judge II", Category = "Weapon", Description = "Upgraded smart version with biometric lock.", CostEurobucks = 1100, Concealability = "J", Availability = "U", Acc = "+1", Damage = "2D6+2", Range = "90m", Capacity = "12", ROF = "3", WeightKg = 3.0 },
        new GearItem { Name = "Nomad Chainsaw Arm", Category = "Weapon", Description = "Retractable chainsaw forearm implant.", CostEurobucks = 2800, Concealability = "N", Availability = "R", Acc = "0", Damage = "4D6", Range = "Melee", Capacity = "-", ROF = "-", WeightKg = 1.0 },
        new GearItem { Name = "Eclipse Phantom", Category = "Weapon", Description = "Ultra-quiet integrated suppressor SMG.", CostEurobucks = 2100, Concealability = "J", Availability = "R", Acc = "+2", Damage = "2D6", Range = "120m", Capacity = "50", ROF = "12", WeightKg = 3.0 },
        new GearItem { Name = "Kang Tao Thunderbolt", Category = "Weapon", Description = "Man-portable railgun for heavy solos.", CostEurobucks = 4500, Concealability = "N", Availability = "R", Acc = "+1", Damage = "4D6+3", Range = "600m", Capacity = "20", ROF = "3", WeightKg = 3.0 },
        new GearItem { Name = "Trauma Team Pacifier", Category = "Weapon", Description = "Non-lethal crowd control for security.", CostEurobucks = 650, Concealability = "N", Availability = "C", Acc = "+1", Damage = "Stun 4D6", Range = "40m", Capacity = "10", ROF = "2", WeightKg = 3.0 },
        new GearItem { Name = "Arasaka Demon", Category = "Weapon", Description = "Mini-grenade launcher disguised as pistol.", CostEurobucks = 1750, Concealability = "J", Availability = "R", Acc = "0", Damage = "Varies", Range = "80m", Capacity = "4", ROF = "1", WeightKg = 3.0 },
        new GearItem { Name = "Viper Fang", Category = "Weapon", Description = "Switchblade with toxin reservoir.", CostEurobucks = 150, Concealability = "P", Availability = "C", Acc = "+2", Damage = "1D6+1", Range = "Melee", Capacity = "-", ROF = "-", WeightKg = 1.0 },
        new GearItem { Name = "Arasaka Executive Vest", Category = "Armor", Description = "Looks like designer suit stops 9mm.", CostEurobucks = 850, SP = "12", EV = "0", WeightKg = 3.0 },
        new GearItem { Name = "Militech Trooper Plate", Category = "Armor", Description = "Full torso trauma plates for corpsec.", CostEurobucks = 1450, SP = "20", EV = "-2", WeightKg = 8.0 },
        new GearItem { Name = "Street Samurai Chrome Jacket", Category = "Armor", Description = "Ballistic mesh with neon chrome studs.", CostEurobucks = 420, SP = "8", EV = "0", WeightKg = 3.0 },
        new GearItem { Name = "Nomad Dust Rider Duster", Category = "Armor", Description = "Kevlar + radiation lining for badlands.", CostEurobucks = 310, SP = "10", EV = "-1", WeightKg = 2.0 },
        new GearItem { Name = "Netrunner Ghost Suit", Category = "Armor", Description = "EMP-shielded with deck ports.", CostEurobucks = 680, SP = "6", EV = "0", WeightKg = 2.0 },
        new GearItem { Name = "Trauma Team Paramedic Vest", Category = "Armor", Description = "High-visibility with med-pouch slots.", CostEurobucks = 920, SP = "14", EV = "-1", WeightKg = 3.0 },
        new GearItem { Name = "Arasaka Black Ops Bodysuit", Category = "Armor", Description = "Chameleon fabric + thermal dampening.", CostEurobucks = 2100, SP = "11", EV = "0", WeightKg = 2.0 },
        new GearItem { Name = "Boostergang Leather & Spikes", Category = "Armor", Description = "Stylish but cheap street protection.", CostEurobucks = 180, SP = "7", EV = "0", WeightKg = 2.0 },
        new GearItem { Name = "Militech Rhino Helmet", Category = "Armor", Description = "With built-in targeting HUD.", CostEurobucks = 650, SP = "18", EV = "-1", WeightKg = 2.0 },
        new GearItem { Name = "Kang Tao Corporate Suit", Category = "Armor", Description = "Bulletproof business wear.", CostEurobucks = 1200, SP = "9", EV = "0", WeightKg = 2.0 },
        new GearItem { Name = "Nomad Road Warrior Plate", Category = "Armor", Description = "Reinforced for high-speed crashes.", CostEurobucks = 890, SP = "13", EV = "-2", WeightKg = 8.0 },
        new GearItem { Name = "Eclipse Shadow Cloak", Category = "Armor", Description = "Optical camo for netrunners.", CostEurobucks = 1450, SP = "5", EV = "0", WeightKg = 2.0 },
        new GearItem { Name = "Street Doc Scrubs + Vest", Category = "Armor", Description = "Practical with multiple pockets.", CostEurobucks = 340, SP = "8", EV = "0", WeightKg = 3.0 },
        new GearItem { Name = "Arasaka Oni Full Plate", Category = "Armor", Description = "Exo-assisted heavy combat suit.", CostEurobucks = 4500, SP = "22", EV = "-3", WeightKg = 8.0 },
        new GearItem { Name = "Fixer’s Silk Armor Lining", Category = "Armor", Description = "Hidden under expensive suits.", CostEurobucks = 550, SP = "10", EV = "0", WeightKg = 2.0 },
        new GearItem { Name = "Runner’s Edge Cyberdeck", Category = "Netrunning", Description = "Entry-level deck with 4 slots.", CostEurobucks = 2200, WeightKg = 1.0 },
        new GearItem { Name = "Trauma Patch", Category = "Medical", Description = "Single-use nanite wound sealer.", CostEurobucks = 120, WeightKg = 0.5 },
        new GearItem { Name = "Ghost Hands Lockpick Set", Category = "Tool", Description = "Neural-feedback electronic picks.", CostEurobucks = 85, WeightKg = 1.0 },
        new GearItem { Name = "Chroma Spray Paint Kit", Category = "Misc", Description = "Neon RFID paint for tagging.", CostEurobucks = 45, WeightKg = 5.0 },
        new GearItem { Name = "Nomad Sun Portable Charger", Category = "Tool", Description = "Solar backpack for cyberware.", CostEurobucks = 210, WeightKg = 1.0 },
        new GearItem { Name = "Black ICE Data Chip", Category = "Netrunning", Description = "One-shot lethal program.", CostEurobucks = 950, WeightKg = 1.0 },
        new GearItem { Name = "Kevlar Duffle Bag", Category = "Misc", Description = "20kg capacity bullet resistant.", CostEurobucks = 60, WeightKg = 1.0 },
        new GearItem { Name = "Vita-Clone Synthetic Blood", Category = "Medical", Description = "Emergency transfusion pack.", CostEurobucks = 300, WeightKg = 0.5 },
        new GearItem { Name = "Ghost Show Holo-Projector", Category = "Tool", Description = "Pocket 3D hologram distractor.", CostEurobucks = 420, WeightKg = 1.0 },
        new GearItem { Name = "Fixer Black Book Comm", Category = "Communication", Description = "Quantum-encrypted burner phone.", CostEurobucks = 180, WeightKg = 1.0 },
        new GearItem { Name = "Arasaka Data Worm", Category = "Netrunning", Description = "Stealth data extraction virus.", CostEurobucks = 650, WeightKg = 1.0 },
        new GearItem { Name = "Medscanner", Category = "Medical", Description = "Handheld diagnostic tool.", CostEurobucks = 420, WeightKg = 0.5 },
        new GearItem { Name = "Grapple Gun", Category = "Tool", Description = "50m monowire with winch.", CostEurobucks = 380, WeightKg = 1.0 },
        new GearItem { Name = "Night Vision Goggles", Category = "Optics", Description = "Thermal + low-light.", CostEurobucks = 290, WeightKg = 1.0 },
        new GearItem { Name = "Forged ID Card", Category = "Misc", Description = "High-quality fake corporate pass.", CostEurobucks = 750, WeightKg = 1.0 },
        new GearItem { Name = "EMP Grenade", Category = "Combat", Description = "One-use electronics killer.", CostEurobucks = 150, WeightKg = 1.0 },
        new GearItem { Name = "Cyberlimb Tool Kit", Category = "Medical", Description = "Portable repair station.", CostEurobucks = 1100, WeightKg = 2.0 },
        new GearItem { Name = "Bug Detector", Category = "Security", Description = "Finds listening devices.", CostEurobucks = 220, WeightKg = 1.0 },
        new GearItem { Name = "Smart Glasses", Category = "Optics", Description = "AR overlay + targeting.", CostEurobucks = 340, WeightKg = 1.0 },
        new GearItem { Name = "Disposable Phone", Category = "Communication", Description = "Burner for one job only.", CostEurobucks = 30, WeightKg = 1.0 },
        new GearItem { Name = "Flashbang Grenade", Category = "Combat", Description = "Non-lethal crowd control.", CostEurobucks = 80, WeightKg = 1.0 },
        new GearItem { Name = "Cyberdeck Cooling Vest", Category = "Netrunning", Description = "Prevents overheating during runs.", CostEurobucks = 450, WeightKg = 1.0 },
        new GearItem { Name = "Nomad Water Purifier", Category = "Survival", Description = "Makes badlands water drinkable.", CostEurobucks = 95, WeightKg = 1.0 },
        new GearItem { Name = "Holo-Map of Night City", Category = "Tool", Description = "Updated street grid projector.", CostEurobucks = 120, WeightKg = 1.0 },
        new GearItem { Name = "Mono-Wire Cutter", Category = "Tool", Description = "Cuts fences and cables silently.", CostEurobucks = 310, WeightKg = 1.0 },
        new GearItem { Name = "Pain Editor Implant Patch", Category = "Medical", Description = "Blocks pain for 6 hours.", CostEurobucks = 80, WeightKg = 0.5 },
        new GearItem { Name = "Corporate Suitcase (armored)", Category = "Misc", Description = "Conceals weapons biometric lock.", CostEurobucks = 650, WeightKg = 1.0 },
        new GearItem { Name = "Street Food Vending Kit", Category = "Misc", Description = "Makes cheap kibble taste edible.", CostEurobucks = 40, WeightKg = 5.0 },
        new GearItem { Name = "Signal Jammer", Category = "Security", Description = "Blocks radio/cell for 50m.", CostEurobucks = 520, WeightKg = 1.0 },
        new GearItem { Name = "Laser Microphone", Category = "Espionage", Description = "Listens through windows.", CostEurobucks = 890, WeightKg = 1.0 },
        new GearItem { Name = "Breath Mask", Category = "Survival", Description = "Filters toxic Night City air.", CostEurobucks = 110, WeightKg = 1.0 },
        new GearItem { Name = "Data Chip Reader/Writer", Category = "Netrunning", Description = "Portable deck accessory.", CostEurobucks = 180, WeightKg = 1.0 },
        new GearItem { Name = "Climbing Spikes (cyber)", Category = "Tool", Description = "Wall-crawling set.", CostEurobucks = 420, WeightKg = 1.0 },
        new GearItem { Name = "Decoy Drone", Category = "Misc", Description = "Remote-controlled distraction.", CostEurobucks = 680, WeightKg = 1.0 },
        new GearItem { Name = "Infrared Binoculars", Category = "Optics", Description = "500m range spotting.", CostEurobucks = 250, WeightKg = 1.0 },
        new GearItem { Name = "Synthcoke (Boost)", Category = "Drug", Description = "+2 REF for 1 hour (addictive).", CostEurobucks = 25, WeightKg = 1.0 },
        new GearItem { Name = "Zero Pain Editor Patch", Category = "Drug", Description = "Total pain block for 6 hours.", CostEurobucks = 80, WeightKg = 1.0 },
        new GearItem { Name = "Smash (hallucinogen)", Category = "Drug", Description = "Euphoric trip popular in clubs.", CostEurobucks = 35, WeightKg = 1.0 },
        new GearItem { Name = "Speedheal Nano", Category = "Medical", Description = "Accelerates healing by 50%.", CostEurobucks = 450, WeightKg = 0.5 },
        new GearItem { Name = "Dorph (combat drug)", Category = "Drug", Description = "+1 BODY ignore 2 wounds.", CostEurobucks = 60, WeightKg = 1.0 },
        new GearItem { Name = "Black Lace", Category = "Drug", Description = "Extreme high with cyberware boost.", CostEurobucks = 120, WeightKg = 1.0 },
        new GearItem { Name = "Airhypo Injector", Category = "Medical", Description = "Needleless drug delivery.", CostEurobucks = 95, WeightKg = 0.5 },
        new GearItem { Name = "Portable Faraday Cage", Category = "Security", Description = "Small bag blocks signals.", CostEurobucks = 310, WeightKg = 1.0 },

        // ── CYBERWARE CATALOGUE ──
        new GearItem { Name = "Interface Plugs",             Category = "Cyberware", Description = "Wrist-mounted neural interface sockets. Required for cyberdeck connection and direct hardwire jacks.",                     CostEurobucks = 200,  Concealability = "N/A", Availability = "Common", WeightKg = 0.1, HumanityCostDice = "2d6" },
        new GearItem { Name = "Chipware Socket",             Category = "Cyberware", Description = "Subdermal processor socket for skill chips and memory expansion. Fits behind the ear.",                                      CostEurobucks = 200,  Concealability = "N/A", Availability = "Common", WeightKg = 0.1, HumanityCostDice = "1d6" },
        new GearItem { Name = "Cyberoptic — Targeting",      Category = "Cyberware", Description = "Replacement eye with built-in targeting reticle. +1 to all ranged attack rolls. Low-light enhancement included.",           CostEurobucks = 900,  Concealability = "N/A", Availability = "Common", WeightKg = 0.1, HumanityCostDice = "2d6" },
        new GearItem { Name = "Cyberoptic — Low-Light",      Category = "Cyberware", Description = "Enhanced optic that amplifies available light. See clearly in near-total darkness.",                                         CostEurobucks = 700,  Concealability = "N/A", Availability = "Common", WeightKg = 0.1, HumanityCostDice = "2d6" },
        new GearItem { Name = "Cyberoptic — Teleoptic",      Category = "Cyberware", Description = "Long-range zoom optic. Up to 50× optical magnification. Useful for scouting and sniper roles.",                             CostEurobucks = 800,  Concealability = "N/A", Availability = "Common", WeightKg = 0.1, HumanityCostDice = "2d6" },
        new GearItem { Name = "Cyberaudio Suite",            Category = "Cyberware", Description = "Full audio replacement: amplified hearing, radio scanner, voice stress analyser, and encrypted comms channel.",              CostEurobucks = 500,  Concealability = "N/A", Availability = "Common", WeightKg = 0.1, HumanityCostDice = "2d6" },
        new GearItem { Name = "Smartgun Link",               Category = "Cyberware", Description = "Neural interface to compatible smartguns. Eliminates iron-sight penalty; target lock via HUD. –2 to all ranged attack DNs.", CostEurobucks = 600,  Concealability = "N/A", Availability = "Common", WeightKg = 0.1, HumanityCostDice = "2d6" },
        new GearItem { Name = "Pain Editor",                 Category = "Cyberware", Description = "Blocks pain signals entirely. Ignore stun penalties from Light and Serious wounds. High Humanity cost.",                     CostEurobucks = 2800, Concealability = "N/A", Availability = "Rare",   WeightKg = 0.2, HumanityCostDice = "2d6" },
        new GearItem { Name = "Sandevistan (Reflex Boost)",  Category = "Cyberware", Description = "Adrenal booster that speeds neural response. +3 to Initiative once per combat. Spine installation.",                        CostEurobucks = 3200, Concealability = "N/A", Availability = "Rare",   WeightKg = 0.2, HumanityCostDice = "2d6" },
        new GearItem { Name = "Kerenzikov Boost",            Category = "Cyberware", Description = "Reflex enhancement that allows action during surprise rounds. +2 Initiative. Cannot combine with Sandevistan.",              CostEurobucks = 2000, Concealability = "N/A", Availability = "Rare",   WeightKg = 0.2, HumanityCostDice = "2d6" },
        new GearItem { Name = "Subdermal Armor (SP4)",       Category = "Cyberware", Description = "Woven carbon-ceramic plates under the skin. SP4 on all body locations; does not stack with worn armor.",                    CostEurobucks = 1200, Concealability = "N/A", Availability = "Rare",   WeightKg = 0.5, HumanityCostDice = "2d6" },
        new GearItem { Name = "Single Cyberarm — Strength",  Category = "Cyberware", Description = "Replacement arm with enhanced servo-motors. +2 to Strength Feat rolls, melee damage +2.",                                   CostEurobucks = 2500, Concealability = "N/A", Availability = "Common", WeightKg = 2.5, HumanityCostDice = "2d6" },
        new GearItem { Name = "Single Cyberleg — Speed",     Category = "Cyberware", Description = "Replacement leg with enhanced actuators. +2 MA for movement calculations.",                                                  CostEurobucks = 2500, Concealability = "N/A", Availability = "Common", WeightKg = 3.0, HumanityCostDice = "2d6" },
        new GearItem { Name = "Linear Frame Beta",           Category = "Cyberware", Description = "Full-body exo-skeletal frame bonded to skeleton. BT treated as 12 for BTM and carry weight. Extreme humanity cost.",        CostEurobucks = 8500, Concealability = "N",   Availability = "Rare",   WeightKg = 5.0, HumanityCostDice = "3d6" },
        new GearItem { Name = "Biomonitor",                  Category = "Cyberware", Description = "Subdermal medical monitor. Tracks vitals, toxins, drug levels. Auto-releases antidotes. Required for Trauma Team card.",     CostEurobucks = 800,  Concealability = "N/A", Availability = "Common", WeightKg = 0.1, HumanityCostDice = "1d6" },
        new GearItem { Name = "Braindance Recorder",         Category = "Cyberware", Description = "Records full sensory experience for playback or broadcast. Neural link required. Media staple.",                             CostEurobucks = 1000, Concealability = "N/A", Availability = "Common", WeightKg = 0.2, HumanityCostDice = "1d6" },
        new GearItem { Name = "Vehicle Link",                Category = "Cyberware", Description = "Direct neural interface to compatible vehicles. +2 to Driving / Pilot rolls when hard-linked.",                              CostEurobucks = 500,  Concealability = "N/A", Availability = "Common", WeightKg = 0.1, HumanityCostDice = "1d6" },

        new GearItem { Name = "Nomad Pack Bike", Category = "Vehicle", Description = "Rugged off-road motorcycle. Nomad favorite.", CostEurobucks = 2000, WeightKg = 300, Seats = "2", TopSpeed = "180 km/h", SDP = "40" },
        new GearItem { Name = "Brennan Apollo", Category = "Vehicle", Description = "Reliable commuter car with basic plating.", CostEurobucks = 12000, WeightKg = 1200, Seats = "4", TopSpeed = "160 km/h", SDP = "50" },
        new GearItem { Name = "Chevillon Thrax", Category = "Vehicle", Description = "Armored corporate transport. Executive luxury.", CostEurobucks = 35000, WeightKg = 2500, Seats = "4", TopSpeed = "150 km/h", SDP = "80" },
        new GearItem { Name = "Quadra Type-66", Category = "Vehicle", Description = "High-performance sports muscle car.", CostEurobucks = 58000, WeightKg = 1800, Seats = "2", TopSpeed = "250 km/h", SDP = "60" },
        new GearItem { Name = "Yaiba Kusanagi", Category = "Vehicle", Description = "Premium sports motorcycle. Incredibly fast.", CostEurobucks = 22000, WeightKg = 350, Seats = "1", TopSpeed = "280 km/h", SDP = "35" },
        new GearItem { Name = "Militech Behemoth", Category = "Vehicle", Description = "Armored personnel carrier.", CostEurobucks = 150000, WeightKg = 6000, Seats = "12", TopSpeed = "110 km/h", SDP = "150" }

    ];


    private async Task SeedTestCharacters()
    {
        var charsCollection = _db.GetCollection<CharacterDocument>("Characters");
        var testCount = await charsCollection.CountDocumentsAsync(c => c.IsTestAsset);
        if (testCount > 0) return;

        var gear = GetStartingGear().ToList();
        
        GearItemDto Map(string name)
        {
            var i = gear.First(x => x.Name == name);
            return new GearItemDto {
                Id = i.Id, Name = i.Name, Category = i.Category, Description = i.Description,
                CostEurobucks = i.CostEurobucks, WeightKg = i.WeightKg,
                Damage = i.Damage, Acc = i.Acc, Range = i.Range, Capacity = i.Capacity,
                ROF = i.ROF, SP = i.SP, EV = i.EV, Seats = i.Seats, TopSpeed = i.TopSpeed, SDP = i.SDP,
                HumanityCostDice = i.HumanityCostDice
            };
        }

        var testAssets = new List<CharacterDocument>
        {
            new CharacterDocument {
                Handle = "RAZOR", Role = "Solo", INT = 6, REF = 10, TECH = 5, COOL = 9, LK = 4, ATT = 6, MA = 8, EMP = 3, BT = 10,
                Humanity = 30, Mobility = 8, Resilience = 10, OwnerId = "SYSTEM", IsTestAsset = true, Eurobucks = 50000,
                EthnicOrigin = "Central American", Clothes = "Combat Jumpsuit", Hairstyle = "Short", Affectation = "Ritual Scars",
                FamilyRanking = "Corporate Executive", FamilyTragedy = "Kidnapped by Corp", PersonalityTrait = "Arrogant",
                Inventory = new() { Map("Quadra Type-66"), Map("Militech Avenger"), Map("Militech Trooper Plate"), Map("Combat Knife"), Map("Subdermal Armor (SP4)"), Map("Pain Editor") }
            },
            new CharacterDocument {
                Handle = "SPIDER", Role = "Netrunner", INT = 10, REF = 7, TECH = 8, COOL = 8, LK = 6, ATT = 5, MA = 6, EMP = 6, BT = 4,
                Humanity = 60, Mobility = 6, Resilience = 4, OwnerId = "SYSTEM", IsTestAsset = true, Eurobucks = 45000,
                EthnicOrigin = "Japanese", Clothes = "High Fashion", Hairstyle = "Colored", Affectation = "Strange Contacts",
                FamilyRanking = "Urban Homeless", FamilyTragedy = "Imprisoned", PersonalityTrait = "Quiet",
                Inventory = new() { Map("Yaiba Kusanagi"), Map("Cyberdeck (Standard)"), Map("Eclipse Arms Specter"), Map("Netrunner Ghost Suit"), Map("Neural Interface (Basic)"), Map("Cyberoptic — Targeting") }
            },
            new CharacterDocument { 
                Handle = "CRASH", Role = "Rockerboy", INT = 7, REF = 8, TECH = 4, COOL = 10, LK = 8, ATT = 9, MA = 7, EMP = 8, BT = 6, 
                Humanity = 80, Mobility = 7, Resilience = 6, OwnerId = "SYSTEM", IsTestAsset = true, Eurobucks = 30000,
                EthnicOrigin = "Anglo-American", Clothes = "Leather Jacket", Hairstyle = "Mohawk", Affectation = "Tattoos",
                FamilyRanking = "Combat Zone", FamilyTragedy = "Missing", PersonalityTrait = "Rebellious",
                Inventory = new() { Map("Brennan Apollo"), Map("Electric Guitar (Custom)"), Map("Medium Pistol (9mm)"), Map("Light Armorjack (SP14)") }
            },
            new CharacterDocument { 
                Handle = "PATCH", Role = "Medtech", INT = 9, REF = 8, TECH = 10, COOL = 7, LK = 5, ATT = 6, MA = 6, EMP = 7, BT = 5, 
                Humanity = 70, Mobility = 6, Resilience = 5, OwnerId = "SYSTEM", IsTestAsset = true, Eurobucks = 40000,
                EthnicOrigin = "European", Clothes = "Corporate Suit", Hairstyle = "Neat", Affectation = "Nervous Habit",
                FamilyRanking = "Corporate Executive", FamilyTragedy = "Exiled", PersonalityTrait = "Compassionate",
                Inventory = new() { Map("Brennan Apollo"), Map("Trauma Team Reaper"), Map("Medscanner (Advanced)"), Map("Trauma Team Paramedic Vest") }
            },
            new CharacterDocument {
                Handle = "GEARHEAD", Role = "Techie", INT = 8, REF = 7, TECH = 10, COOL = 6, LK = 7, ATT = 4, MA = 5, EMP = 5, BT = 6,
                Humanity = 50, Mobility = 5, Resilience = 6, OwnerId = "SYSTEM", IsTestAsset = true, Eurobucks = 35000,
                EthnicOrigin = "Russian", Clothes = "Work Clothes", Hairstyle = "Greasy", Affectation = "Tools on Belt",
                FamilyRanking = "Arcology", FamilyTragedy = "Accident", PersonalityTrait = "Practical",
                Inventory = new() { Map("Nomad Pack Bike"), Map("Basic Tool Kit"), Map("Viper Dynamics Street Judge"), Map("Street Samurai Chrome Jacket"), Map("Interface Plugs"), Map("Biomonitor") }
            },
            new CharacterDocument { 
                Handle = "SUIT", Role = "Corporate", INT = 9, REF = 6, TECH = 5, COOL = 10, LK = 8, ATT = 8, MA = 6, EMP = 8, BT = 5, 
                Humanity = 80, Mobility = 6, Resilience = 5, OwnerId = "SYSTEM", IsTestAsset = true, Eurobucks = 150000,
                EthnicOrigin = "European", Clothes = "Designer Suit", Hairstyle = "Immaculate", Affectation = "High-End Watch",
                FamilyRanking = "Corporate Royalty", FamilyTragedy = "Betrayal", PersonalityTrait = "Calculating",
                Inventory = new() { Map("Chevillon Thrax"), Map("Arasaka Black Widow"), Map("Arasaka Executive Vest"), Map("Corporate SmartCard (1000 eb)") }
            },
            new CharacterDocument { 
                Handle = "BADGE", Role = "Cop", INT = 7, REF = 8, TECH = 5, COOL = 9, LK = 5, ATT = 6, MA = 7, EMP = 6, BT = 8, 
                Humanity = 60, Mobility = 7, Resilience = 8, OwnerId = "SYSTEM", IsTestAsset = true, Eurobucks = 25000,
                EthnicOrigin = "Anglo-American", Clothes = "Uniform", Hairstyle = "Flat-top", Affectation = "Cigar",
                FamilyRanking = "Urban Middle Class", FamilyTragedy = "Murdered", PersonalityTrait = "Honest",
                Inventory = new() { Map("Brennan Apollo"), Map("Medium Pistol (Lawforce Issue)"), Map("Medium Armor (SP18)"), Map("Handcuffs (Smart)") }
            },
            new CharacterDocument { 
                Handle = "DRIFTER", Role = "Nomad", INT = 6, REF = 9, TECH = 8, COOL = 8, LK = 7, ATT = 5, MA = 8, EMP = 7, BT = 7, 
                Humanity = 70, Mobility = 8, Resilience = 7, OwnerId = "SYSTEM", IsTestAsset = true, Eurobucks = 20000,
                EthnicOrigin = "African", Clothes = "Road Leathers", Hairstyle = "Braids", Affectation = "Dusty Goggles",
                FamilyRanking = "Nomad Pack", FamilyTragedy = "Scattered", PersonalityTrait = "Loyal",
                Inventory = new() { Map("Nomad Pack Bike"), Map("Nomad Sawtooth"), Map("Nomad Dust Rider Duster"), Map("Basic Tool Kit") }
            }
        };

        foreach(var asset in testAssets)
        {
            var vehicle = asset.Inventory.FirstOrDefault(i => i.Category == "Vehicle");
            if (vehicle != null) asset.EquippedGearIds["VEHICLE_SLOT"] = vehicle.InstanceId;
            
            var weapon = asset.Inventory.FirstOrDefault(i => i.Category == "Weapon");
            if (weapon != null) asset.EquippedGearIds["WEAPON"] = weapon.InstanceId;

            var armor = asset.Inventory.FirstOrDefault(i => i.Category == "Armor");
            if (armor != null) asset.EquippedGearIds["TORSO"] = armor.InstanceId;
        }

        await charsCollection.InsertManyAsync(testAssets);
    }
}
