# CSYM505

Σύστημα BI και analytics για την υποστήριξη στρατηγικών αποφάσεων ενός νέου εποχικού ξενοδοχείου στα Πυργαδίκια Χαλκιδικής.

Το ξενοδοχείο δεν διαθέτει ακόμη ιστορικό λειτουργίας. Η μελέτη βασίζεται σε πραγματικά τουριστικά δεδομένα της Eurostat για ζήτηση, εποχικότητα, αφίξεις, διανυκτερεύσεις, δυναμικότητα, πληρότητα και δραστηριότητα online πλατφορμών.

## Τι έχουμε

- Δέκα πλήρη datasets σε πρωτογενή TSV στο [etl/data/raw](etl/data/raw).
- Ένα Apache Hop project στο [etl](etl), με δέκα ολοκληρωμένα [pipelines](etl/pipeline).
- Δέκα CSV στο [etl/data/processed](etl/data/processed), με πλήρη έλεγχο συμφωνίας με τα αρχικά TSV ως προς κλειδιά, τιμές, nulls, flags και ακριβές raw περιεχόμενο. Οι στήλες, οι περίοδοι και οι αριθμοί γραμμών βρίσκονται στον [κατάλογο datasets](docs/dataset-register.md).
- Δέκα imports CSV στην εφαρμογή C#/.NET, για τα `tour_occ_nin3`, `tour_occ_nin2m`, `tour_occ_nim`, `tour_occ_arm`, `tour_occ_arn2`, `tour_ce_omn12`, `tour_ce_omr`, `tour_ce_oan3`, `tour_cap_nuts2` και `tour_occ_anor2`, με entities, configurations και εφαρμοσμένα migrations στην τοπική SQLite.

Τα διαθέσιμα αρχεία περιλαμβάνουν όλες τις περιοχές και τις κατηγορίες που δημοσιεύονται σε κάθε dataset. Στα δέκα imports αποθηκεύονται μόνο οι ελληνικές γεωγραφίες (`geo` που ξεκινά από `EL`), με όλες τις διαθέσιμες μονάδες, δείκτες και κατηγορίες κάθε dataset. Τα φίλτρα ανάλυσης θα οριστούν στο επόμενο στάδιο. Στα πρωτογενή TSV/CSV του `tour_occ_anor2` διατηρούνται δύο τιμές PC πάνω από 100 για τη Σλοβακία, τεκμηριωμένες στον κατάλογο datasets. Δεν εισάγονται στην ελληνική βάση.

## Νέο πλάνο

```text
Πρωτογενή TSV της Eurostat
          ↓
Μετασχηματισμός με Apache Hop
          ↓
CSV με τις απαραίτητες στήλες και διατηρημένα στοιχεία ποιότητας
          ↓
Φιλτράρισμα και μετατροπή σε entities από την εφαρμογή
          ↓
Αποθήκευση σε βάση δεδομένων
          ↓
Ανάλυση, δείκτες, προβλέψεις και επιχειρηματικές προτάσεις
```

Η δουλειά προχωρά ένα dataset και ένα βήμα κάθε φορά. Και τα δέκα imports έχουν υλοποιηθεί. Επόμενο στάδιο είναι ο ορισμός φίλτρων και δεικτών ανάλυσης.

## Εισαγωγή CSV

Το `POST /api/import-data` καλεί το [ImportDataService](src/CSYM505.Api/Controllers/ImportDataApi/Services/ImportDataService.cs) και εισάγει διαδοχικά:

| Dataset | Entity / πίνακας | Ελληνικές γραμμές |
|---|---|---:|
| tour_occ_nin3 | AnnualAccommodationNights / tour_occ_nin3 | 7.176 |
| tour_occ_nin2m | MonthlyAccommodationNights / tour_occ_nin2m | 81.900 |
| tour_occ_nim | NationalMonthlyAccommodationNights / tour_occ_nim | 22.440 |
| tour_occ_arm | NationalMonthlyAccommodationArrivals / tour_occ_arm | 22.440 |
| tour_occ_arn2 | AnnualAccommodationArrivals / tour_occ_arn2 | 19.332 |
| tour_ce_omn12 | MonthlyPlatformActivity / tour_ce_omn12 | 20.952 |
| tour_ce_omr | NationalMonthlyPlatformActivity / tour_ce_omr | 1.053 |
| tour_ce_oan3 | AnnualPlatformGuestNights / tour_ce_oan3 | 1.296 |
| tour_cap_nuts2 | AnnualAccommodationCapacity / tour_cap_nuts2 | 26.196 |
| tour_occ_anor2 | AnnualHotelOccupancy / tour_occ_anor2 | 504 |

Οι διαδρομές υπολογίζονται από τον φάκελο του API προς το `etl/data/processed`. Ο helper χρησιμοποιεί τον [TextFieldParser της .NET](https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualbasic.fileio.textfieldparser?view=net-10.0) και δίνει τις γραμμές διαδοχικά στο `foreach`. Το service παραλείπει τις μη ελληνικές γεωγραφίες και συγκεντρώνει μόνο τα επιλεγμένα entities πριν από οποιαδήποτε διαγραφή.

Κάθε dataset έχει ξεχωριστό transaction. Διαγράφεται μόνο ο αντίστοιχος πίνακας και εισάγονται παρτίδες 5.000 εγγραφών με `SaveChangesAsync()` και `ChangeTracker.Clear()`. Το commit γίνεται μετά την τελευταία παρτίδα. Κενό αρχείο, απουσία ελληνικών γραμμών ή μη έγκυρες επιλεγμένες γραμμές απορρίπτονται. Αποτυχία οποιασδήποτε παρτίδας επαναφέρει τον συγκεκριμένο πίνακα· προηγούμενο επιτυχημένο dataset παραμένει αποθηκευμένο.

Στο nin2m διατηρούνται οι μήνες M01–M12 και το ετήσιο TOTAL χωριστά, με `freq=A` όπως στην πηγή. Στα nim και arm διατηρείται η μηνιαία περίοδος `time_period` σε μορφή `YYYY-MM`, με `freq=M`, από 1990-01 έως 2026-08. Στο arn2 διατηρούνται τα έτη 1990–2025 με `freq=A` και οι 18 ελληνικές γεωγραφίες της πηγής. Στα omn12 και omr διατηρούνται οι δείκτες LSTY/NGT_SP/STY και οι μήνες M01–M12/TOTAL, με `freq=A` και έτη 2018–2026. Στο oan3 διατηρούνται ο δείκτης NGT_SP και τα έτη 2018–2025, χωρίς διάσταση μήνα, σε 54 ελληνικές γεωγραφίες. Στο cap_nuts2 διατηρούνται χωριστά BEDPL/BEDRM/ESTBL και οι μονάδες NR/PCH_PRE, για 1990–2026. Στο anor2 αποθηκεύεται η ετήσια πληρότητα κλινών/δωματίων για 2012–2025, με μονάδα PC. Οι τιμές κρατούν τα δεκαδικά τους, χωρίς αλλαγή κλίμακας, και επιλεγμένες τιμές εκτός 0–100 απορρίπτονται πριν από τη διαγραφή. Και στα δέκα imports κρατάμε όλες τις διαθέσιμες μονάδες και κατηγορίες κάθε dataset, nulls, μηδενικά, flags και το ακριβές `raw_value`. Το `Value` είναι `decimal?` και αποθηκεύεται ως TEXT από τον SQLite provider για διατήρηση της ακρίβειας ([τύποι SQLite/.NET](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/types)).

Τα SQL command logs παραμένουν στο `Warning` στα `appsettings.json` και `appsettings.Development.json`, ώστε οι μαζικές εισαγωγές να μην τυπώνουν κάθε SQL εντολή στον debugger.

Τα migrations `AddTourOccNin3`, `AddTourOccNin2m`, `AddTourOccNim`, `AddTourOccArm`, `AddTourOccArn2`, `AddTourCeOmn12`, `AddTourCeOmr`, `AddTourCeOan3`, `AddTourCapNuts2` και `AddTourOccAnor2` έχουν εφαρμοστεί στην τοπική [csym505.db](src/CSYM505.Api/csym505.db). Στις 23/10/2026 ολοκληρώθηκαν και τα δέκα imports στην τοπική βάση, συμπεριλαμβανομένου του `tour_occ_anor2` με 504 εγγραφές. Και οι δέκα πίνακες επαληθεύτηκαν πλήρως απέναντι στις ελληνικές γραμμές των CSV ως προς κλειδιά, τιμές, nulls, flags και raw περιεχόμενο, χωρίς διαφορές ή διπλότυπα. Σε προσωρινή SQLite επαληθεύτηκαν όλα τα κλειδιά, τιμές, nulls, flags και raw περιεχόμενο και των δέκα datasets, η επανεισαγωγή, η απόρριψη μη έγκυρων διαστάσεων/περιόδων/τιμών και το rollback σε δεύτερη παρτίδα. Για την πληρότητα ελέγχθηκαν δεκαδικά ποσοστά, τα όρια 0–100 και η παράλειψη μη ελληνικών παρατηρήσεων πριν από τον έλεγχο τιμών. Πέρασε και η δοκιμή HTTP 200 με τα δέκα αποτελέσματα.

Κάνε επανεκκίνηση του API και εκτέλεσε το [αρχείο HTTP](src/CSYM505.Api/CSYM505.Api.http). Η αναμενόμενη απάντηση είναι πλέον λίστα:

```json
[
  {
    "dataset": "tour_occ_nin3",
    "importedRows": 7176
  },
  {
    "dataset": "tour_occ_nin2m",
    "importedRows": 81900
  },
  {
    "dataset": "tour_occ_nim",
    "importedRows": 22440
  },
  {
    "dataset": "tour_occ_arm",
    "importedRows": 22440
  },
  {
    "dataset": "tour_occ_arn2",
    "importedRows": 19332
  },
  {
    "dataset": "tour_ce_omn12",
    "importedRows": 20952
  },
  {
    "dataset": "tour_ce_omr",
    "importedRows": 1053
  },
  {
    "dataset": "tour_ce_oan3",
    "importedRows": 1296
  },
  {
    "dataset": "tour_cap_nuts2",
    "importedRows": 26196
  },
  {
    "dataset": "tour_occ_anor2",
    "importedRows": 504
  }
]
```

## Στόχος της μελέτης

Να υποστηριχθούν αποφάσεις για την περίοδο λειτουργίας, τις αγορές-στόχους, την εποχική διάθεση δωματίων, την τιμολογιακή στρατηγική και τις αρχικές πολιτικές ακύρωσης.

Η αρχική μελέτη εστιάζει σε Ελλάδα, Κεντρική Μακεδονία και Χαλκιδική, κυρίως στην περίοδο 2020–2025 όπου υπάρχουν συμβατά δεδομένα. Τα στοιχεία περιγράφουν την αγορά· δεν αποτελούν κρατήσεις ή απόδοση του ίδιου του ξενοδοχείου.

- [Επιχειρηματικό πλάνο](docs/project-plan.md)
- [Διαθέσιμα datasets και σημασία των μετρήσεων](docs/dataset-register.md)
- [Πλαίσιο συνεργασίας](AGENTS.md)
