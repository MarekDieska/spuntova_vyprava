# Save the World — Planetárna výprava

Samostatná hrateľná HTML5 rekonštrukcia podľa dodaného REA reportu. Nevyžaduje Unity, inštaláciu balíkov ani internet.

## Spustenie

Otvor `Save-the-World.html` v aktuálnom Chrome, Edge alebo Firefoxe. Všetky skripty, štýly a dátový katalóg sú vložené v tomto jednom súbore.

Alternatívne v tomto priečinku spusti `npm start` alebo `node serve.cjs` a otvor `http://127.0.0.1:4173`. Na statický webhosting možno nahrať obsah priečinka `dist`.

## Ovládanie a cieľ

- WASD / šípky: pohyb po obrazovke.
- Drž medzerník: zber pri zdroji alebo útok na blízkeho nepriateľa.
- E: interakcia s jadrom planéty alebo táborom.
- I: batoh, U: dielňa, R: rafinérie, H: domov, J: denník, M: mapa.
- Esc: zatvorenie menu / pauza.
- Dotyk: ľavý joystick, tlačidlo nástroja a E. Na úzkych obrazovkách sa zobrazujú automaticky.

Nazbieraj 20 dreva na Hadese. Príď ku kryštálu, stlač E a obnov planétu. Hviezdna mapa sprístupní Gaiu. Ďalšie svety vyžadujú viac surovín, dosky a oceľ. Obnova všetkých desiatich planét je víťazstvom; potom môžeš dobrovoľne spustiť novú výpravu s prestige bonusom.

Výroba: 30 dreva → 10 dosiek za 30 sekúnd a 40 kameňa → 8 ocele za 40 sekúnd. Každá rafinéria má tri miesta vo fronte. Tábor lieči, domov zvyšuje maximálne zdravie a pomocníci zbierajú drevo a kameň. Pri porážke sa vrátiš do tábora a stratíš 10 % mincí.

## Uloženie

Postup sa automaticky ukladá do localStorage daného prehliadača. Súbor otvorený cez `file:` a serverová adresa môžu mať samostatné uloženia. Pri obmedzenom úložisku použi **Denník → Export uloženia**. Export/import funguje cez JSON a umožňuje preniesť postup medzi prehliadačmi. Výroba pokračuje podľa času; pomocníci započítajú najviac hodinu offline práce.

## Rozsah rekonštrukcie

Implementované: desať odomykateľných planét, procedurálna izometrická grafika, pohyb podľa terénu, zber a obnova zdrojov, boj a strážcovia, nástroje, brnenie, dve rafinérie, tábor, pomocníci, zberateľské výzvy, prestige, zvuk a uloženie.

Archív v denníku obsahuje všetkých 4 907 dekódovaných objektov vrátane 609 surovinových záznamov, 283 nepriateľov a 95 receptov. Ich prítomnosť v archíve neznamená, že každý je aktívnou hernou mechanikou. Nepoužité varianty, ďalšie recepty, pôvodné questy a konfigurácia zostávajú dostupné na preskúmanie a ďalší vývoj.

Report neobsahuje pôvodnú vykonateľnú IL2CPP logiku. Ide o novú implementáciu, nie presný port originálu. Grafika a mapy sú vytvorené pre tento build. Konkrétne hranice a pôvod dát opisuje `DATA_SOURCES.md`.

## Vývoj a overenie

- `npm test`: 10 testov ekonomiky, postupu, uloženia, pohybu, zberu, boja a UI akcií.
- `npm run build`: syntaktická kontrola JavaScriptu a vytvorenie samostatného HTML súboru.
- `node tools/extract-data.cjs "cesta/k/rea-savetheworld-report"`: znovu vytvorí dátový katalóg z reportu, ktorý číta bez úprav.

Desktopový prehliadač bol overený pre spustenie, pohyb, zber, menu, načítanie uloženia a vyhľadávanie v archíve. Mobilné rozloženie bolo vizuálne skontrolované; fyzický dotykový telefón nebol testovaný. Prechod všetkých planét bol overený automatickým testom ekonomiky, nie úplným ručným dohraním.
