# CSYM505

Σύστημα BI και analytics για την υποστήριξη στρατηγικών αποφάσεων ενός νέου εποχικού ξενοδοχείου στα Πυργαδίκια Χαλκιδικής.

Το ξενοδοχείο δεν διαθέτει ακόμη ιστορικό λειτουργίας. Η μελέτη βασίζεται σε πραγματικά τουριστικά δεδομένα της Eurostat και της Τράπεζας της Ελλάδος για ζήτηση, εποχικότητα, αφίξεις, διανυκτερεύσεις, δυναμικότητα, πληρότητα και δραστηριότητα online πλατφορμών.

## Τι έχουμε

- Δέκα πλήρη datasets Eurostat σε TSV στο [etl/data/raw](etl/data/raw) και τρία Excel Τράπεζας της Ελλάδος στο [bank-of-greece](etl/data/raw/bank-of-greece).
- Ένα Apache Hop project στο [etl](etl), με δεκατρία ολοκληρωμένα [pipelines](etl/pipeline).
- Δεκατρία CSV στο [etl/data/processed](etl/data/processed): δέκα επαληθευμένα έναντι TSV και τρία έναντι ετήσιων Excel, με διατήρηση διαστάσεων, τιμών, nulls, flags και αριθμητικής ακρίβειας. Οι στήλες, οι περίοδοι και οι αριθμοί γραμμών βρίσκονται στον [κατάλογο datasets](docs/dataset-register.md).
- Δέκα imports Eurostat στην εφαρμογή C#/.NET, για τα `tour_occ_nin3`, `tour_occ_nin2m`, `tour_occ_nim`, `tour_occ_arm`, `tour_occ_arn2`, `tour_ce_omn12`, `tour_ce_omr`, `tour_ce_oan3`, `tour_cap_nuts2` και `tour_occ_anor2`, με entities, configurations και εφαρμοσμένα migrations στην τοπική SQLite.
- Τρία επιπλέον imports Τράπεζας της Ελλάδος, με 54 εγγραφές το καθένα για EL52/2020–2025, και το SQL view `analytics_regional_travel_receipts_share`.

Τα διαθέσιμα αρχεία περιλαμβάνουν όλες τις περιοχές και τις κατηγορίες που δημοσιεύονται σε κάθε dataset. Στα δέκα imports αποθηκεύονται μόνο οι ελληνικές γεωγραφίες (`geo` που ξεκινά από `EL`), με όλες τις διαθέσιμες μονάδες, δείκτες και κατηγορίες κάθε dataset. Τα φίλτρα και οι δείκτες ανάλυσης έχουν συμφωνηθεί στο [πλάνο AS1](docs/as1-agreed-plan.md) και έχουν υλοποιηθεί και αποθηκευτεί και οι οκτώ K1–K8. Στα πρωτογενή TSV/CSV του `tour_occ_anor2` διατηρούνται δύο τιμές PC πάνω από 100 για τη Σλοβακία, τεκμηριωμένες στον κατάλογο datasets. Δεν εισάγονται στην ελληνική βάση.

## Συμφωνημένη ροή

```text
Πρωτογενή TSV Eurostat και ετήσια δεδομένα Excel Τράπεζας της Ελλάδος
          ↓
Μετασχηματισμός με Apache Hop
          ↓
CSV με τις απαραίτητες στήλες και διατηρημένα στοιχεία ποιότητας
          ↓
Φιλτράρισμα και μετατροπή σε entities από την εφαρμογή
          ↓
Αποθήκευση σε βάση δεδομένων
          ↓
Δείκτες C#/.NET, SQLite view μεριδίων και στατιστικά Math.NET
          ↓
Δύο προβλέψεις με ML.NET και αξιολόγηση έναντι seasonal naive
          ↓
Dashboard Apache Superset, επιχειρηματικές προτάσεις και αναφορά AS1
```

Η δουλειά προχωρά ένα συγκεκριμένο βήμα κάθε φορά, με τον χρήστη να προσθέτει τον κώδικα και να εκτελεί τις εντολές. Στις 25/10/2026 έχουν ολοκληρωθεί τα 13 imports, οι K1–K8, η ένταση εποχικότητας Math.NET και η συμπληρωματική ανάλυση αγορών προέλευσης. Το Superset παρουσιάζει 23 οπτικοποιήσεις (19 γραφήματα και τέσσερις κάρτες) σε τέσσερις καρτέλες. Επόμενο βήμα είναι η προετοιμασία και ο έλεγχος των δύο σειρών EL52 πριν από την αυτοσυσχέτιση και τις προβλέψεις.

## Συμφωνημένο αντικείμενο AS1

Το [συμφωνημένο πλάνο και οι απαιτήσεις AS1](docs/as1-agreed-plan.md) είναι το κεντρικό σημείο αναφοράς για το αντικείμενο, τους περιορισμούς και τα κριτήρια ολοκλήρωσης:

- **Οκτώ δείκτες αγοράς K1–K8:** ζήτηση, εποχικότητα, σύνθεση επισκεπτών, εθνική ξενοδοχειακή διάρκεια παραμονής, δωμάτια, πληρότητα και guest nights πλατφορμών.
- **Δύο ανεξάρτητες μηνιαίες προβλέψεις F1/F2 για EL52:** διανυκτερεύσεις καταλυμάτων και guest nights πλατφορμών, SSA μέσω ML.NET έναντι seasonal naive, ορίζοντας 12 μηνών.
- **Συμπληρωματικά στατιστικά Math.NET:** ένταση εποχικότητας και αυτοσυσχέτιση με έμφαση στους 12 μήνες.
- **Αγορές προέλευσης από Τράπεζα της Ελλάδος:** μερίδιο περιφερειακών εισπράξεων, δημοσιευμένη δαπάνη ανά διανυκτέρευση και διάρκεια ανά επίσκεψη, χωρίς αλλαγή αρίθμησης K1–K8.
- **Ένα dashboard με πέντε προβλεπόμενες καρτέλες:** Ζήτηση και επισκέπτες, Εποχικότητα, Προσφορά και online πλατφόρμες, Αγορές προέλευσης, Προβλέψεις. Υπάρχουν οι τέσσερις πρώτες· η «Προβλέψεις» εκκρεμεί.

Το `POST /api/analyze-data` υπολογίζει και αποθηκεύει τους K1–K8 και επιστρέφει `AnalyticsResult` με οκτώ λίστες: `AnnualDemand`, `MonthlyDemand`, `AnnualSeasonality`, `VisitorComposition`, `MonthlyLengthOfStay`, `AnnualHotelRooms`, `AnnualRoomOccupancy` και `AnnualPlatformDemand`. Το MathNet.Numerics 5.0.0 υπολογίζει μέση μηνιαία ζήτηση, πληθυσμιακή τυπική απόκλιση και συντελεστή μεταβλητότητας στις ίδιες 14 εγγραφές της `AnnualSeasonality`. Έχουν εφαρμοστεί τα αντίστοιχα migrations:

| Ανάλυση | Πίνακας αποτελεσμάτων | Επαληθευμένες εγγραφές |
|---|---|---:|
| K1 | analytics_annual_demand | 18 |
| K2 | analytics_monthly_demand | 168 |
| K3 και στατιστικά Math.NET | analytics_annual_seasonality | 14 |
| K4 | analytics_visitor_composition | 14 |
| K5 | analytics_monthly_length_of_stay | 72 |
| K6 | analytics_annual_hotel_rooms | 14 |
| K7 | analytics_annual_room_occupancy | 12 |
| K8 | analytics_annual_platform_demand | 18 |

Οι τιμές, τα flags και το διαθέσιμο raw περιεχόμενο συμφωνούν με τις πηγές και οι δείκτες με ανεξάρτητους δεκαδικούς υπολογισμούς. Τα στατιστικά Math.NET επαληθεύτηκαν με ανοχή για double. Για K2/K3 συγκρίθηκαν τα IDs μεταξύ εκτελέσεων. Για K4–K8, μετά από δύο εκτελέσεις κάθε βήματος επιβεβαιώθηκαν τα αναμενόμενα πλήθη, μοναδικά κλειδιά και μετρητές αυτοαρίθμησης, χωρίς ένδειξη επανεισαγωγής· δεν υπήρξε χωριστό στιγμιότυπο ανάμεσα σε κάθε ζεύγος εκτελέσεων. Εκκρεμούν η προετοιμασία και ο έλεγχος των σειρών F1/F2, η αυτοσυσχέτιση, τα πειράματα πρόβλεψης και η καρτέλα «Προβλέψεις», οι πρόσθετες δοκιμές οριακών περιπτώσεων, η παρουσίαση flags και η τελική επιχειρηματική ερμηνεία/αναφορά. Οι λεπτομέρειες διατηρούνται στο [πλάνο αναλύσεων](docs/analysis-plan.md).

## Τοπικό Superset και ενημέρωση δεδομένων

Το [compose.yaml](superset/compose.yaml) χρησιμοποιεί `apache/superset:6.1.0` και εκθέτει το Superset μόνο στο [localhost:8088](http://localhost:8088). Τα μεταδεδομένα του Superset διατηρούνται στο volume `superset_home`, χωριστά από την `csym505.db`. Το μυστικό κλειδί βρίσκεται στο αγνοημένο `superset/.env` και δεν καταγράφεται στην τεκμηρίωση.

Η υπάρχουσα SQLite προσαρτάται μόνο για ανάγνωση στο `/app/data/csym505.db`. Η σύνδεση δεδομένων χρησιμοποιεί:

```text
sqlite:///file:/app/data/csym505.db?mode=ro&immutable=1&uri=true
```

Με αυτή τη σύνδεση δουλεύουμε με στατική βάση. Πριν από κάθε ενημέρωση δεδομένων:

1. Σταματάμε το Superset πριν εκκινήσουμε την εφαρμογή .NET:

   ```sh
   docker compose -f /Users/ioannis/Documents/CSYM505/superset/compose.yaml stop superset
   ```

2. Εκτελούμε την εφαρμογή και την εισαγωγή ή ανάλυση που θέλουμε και έπειτα σταματάμε την εφαρμογή .NET.
3. Με την εφαρμογή σταματημένη, μεταφέρουμε τις αλλαγές του WAL στο κύριο αρχείο της βάσης:

   ```sh
   sqlite3 "/Users/ioannis/Documents/CSYM505/src/CSYM505.Api/csym505.db" "PRAGMA wal_checkpoint(TRUNCATE);"
   ```

   Η επιτυχής ολοκλήρωση που επαληθεύτηκε είναι `0|0|0`. Αν ο έλεγχος είναι απασχολημένος ή αποτύχει, δεν προχωράμε στην εκκίνηση του Superset.
4. Εκκινούμε ξανά το Superset και ανανεώνουμε τα γραφήματα:

   ```sh
   docker compose -f /Users/ioannis/Documents/CSYM505/superset/compose.yaml up -d
   ```

Έχουν καταχωριστεί 21 datasets χωρίς αντιγραφή δεδομένων: δέκα πίνακες Eurostat, οκτώ πίνακες K1–K8, το view εισπράξεων και οι πίνακες δαπάνης/διάρκειας Τράπεζας της Ελλάδος. Το dashboard παραμένει Draft, με κατανομή οπτικοποιήσεων 9/4/7/3 στις τέσσερις υπάρχουσες καρτέλες. Τα φίλτρα Περιοχή/Έτος εφαρμόζονται μόνο στις τέσσερις κάρτες σύνοψης. Το «Έτος αγορών προέλευσης» είναι ξεχωριστό, υποχρεωτικό φίλτρο ενός έτους, αρχικά 2025, με scope μόνο τα τρία νέα γραφήματα και χωρίς σταθερό έτος μέσα τους. Η αποθήκευση, τοποθέτηση, επεξηγήσεις και λειτουργία φίλτρων επιβεβαιώθηκαν από τον χρήστη. Οι K1–K8 παραμένουν στο Analytics Service και το νέο μερίδιο εισπράξεων σε SQLite view, ανεξάρτητα από το εργαλείο BI. Η [διάταξη και συμπεριφορά φίλτρων](docs/analysis-plan.md#εργαλεία-εφαρμογή-και-dashboard) διατηρείται στο πλάνο αναλύσεων.

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
| bog_regional_travel_receipts | AnnualRegionalTravelReceipts / bog_regional_travel_receipts | 54 |
| bog_regional_expenditure_per_night | AnnualRegionalExpenditurePerNight / bog_regional_expenditure_per_night | 54 |
| bog_regional_length_of_stay | AnnualRegionalLengthOfStay / bog_regional_length_of_stay | 54 |

Οι διαδρομές υπολογίζονται από τον φάκελο του API προς το `etl/data/processed`. Ο helper χρησιμοποιεί τον [TextFieldParser της .NET](https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualbasic.fileio.textfieldparser?view=net-10.0) και δίνει τις γραμμές διαδοχικά στο `foreach`. Στα δέκα Eurostat imports το service παραλείπει τις μη ελληνικές γεωγραφίες· στα τρία Τράπεζας της Ελλάδος επιλέγει μόνο EL52/2020–2025. Συγκεντρώνει τα επιλεγμένα entities πριν από οποιαδήποτε διαγραφή.

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
  },
  {
    "dataset": "bog_regional_travel_receipts",
    "importedRows": 54
  },
  {
    "dataset": "bog_regional_expenditure_per_night",
    "importedRows": 54
  },
  {
    "dataset": "bog_regional_length_of_stay",
    "importedRows": 54
  }
]
```

## Τράπεζα της Ελλάδος και αγορές προέλευσης

Η πρόσθετη ροή είναι Excel → Hop → CSV → υπάρχον `POST /api/import-data` → SQLite → view/δημοσιευμένοι δείκτες → Superset. Το endpoint εκτελεί πρώτα τα δέκα Eurostat και μετά τα τρία νέα imports, επιστρέφοντας 13 αποτελέσματα. Εφαρμόστηκαν τα `AddBankOfGreeceRegionalData` και `AddRegionalTravelReceiptsShareView`. Δεν προστέθηκε νέο endpoint ή λίστα στο AnalyticsResult.

Το `analytics_regional_travel_receipts_share` δίνει 100 × εισπράξεις χώρας / περιφερειακό TOTAL για ίδιο έτος και διαστάσεις, με null για άγνωστο ή μηδενικό παρονομαστή. Δαπάνη και διάρκεια διαβάζονται όπως δημοσιεύονται. Τα στοιχεία αφορούν επισκέπτες εξωτερικού στην EL52, όχι χωριστά Χαλκιδική. Οι επιλεγμένες χώρες της πηγής δεν αποτελούν πλήρη κατάταξη· τα μερίδια δεν καλύπτουν αναγκαστικά το 100%. Απουσία τιμής δεν σημαίνει μηδέν. Η δαπάνη δεν είναι ADR και η διάρκεια ανά περιφερειακή επίσκεψη διαφέρει από τον εθνικό ξενοδοχειακό K5.

Οι [πηγές, τα pipelines και η κάλυψη](docs/dataset-register.md#τράπεζα-της-ελλάδος-περιφερειακά-δεδομένα) και οι [υπολογισμοί και έλεγχοι](docs/analysis-plan.md#αγορές-προέλευσης-τράπεζα-της-ελλάδος) καταγράφονται στα υπάρχοντα έγγραφα.

## Στόχος της μελέτης

Να υποστηριχθούν αποφάσεις για την περίοδο λειτουργίας, τη στελέχωση, τη στόχευση επισκεπτών, τα πακέτα διαμονής και τις προτεραιότητες προβολής, με κατανόηση της ξενοδοχειακής προσφοράς και της δραστηριότητας πλατφορμών.

Η αρχική μελέτη εστιάζει σε Ελλάδα, Κεντρική Μακεδονία και Χαλκιδική, κυρίως στην περίοδο 2020–2025 όπου υπάρχουν συμβατά δεδομένα. Τα στοιχεία περιγράφουν την αγορά· δεν αποτελούν κρατήσεις ή απόδοση του ίδιου του ξενοδοχείου.

Η πληρότητα του ίδιου του ξενοδοχείου, ADR, RevPAR, έσοδα, ακυρώσεις και μερίδιο άμεσων πωλήσεων είναι εκτός του συμφωνημένου πεδίου. Προτάσεις τιμολόγησης και εμπορικής πολιτικής παραμένουν στρατηγικές λόγω απουσίας λειτουργικών δεδομένων.

- [Συμφωνημένο πλάνο και απαιτήσεις AS1](docs/as1-agreed-plan.md)
- [Αρχική εκφώνηση AS1](docs/references/as1-specification.txt)
- [Επιχειρηματικό πλάνο](docs/project-plan.md)
- [Πλάνο αναλύσεων, επιχειρηματικά ερωτήματα και KPI](docs/analysis-plan.md)
- [Διαθέσιμα datasets και σημασία των μετρήσεων](docs/dataset-register.md)
- [Πλαίσιο συνεργασίας](AGENTS.md)
