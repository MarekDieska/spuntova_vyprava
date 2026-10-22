# Dáta reportu a hranice rekonštrukcie

`tools/extract-data.cjs` načíta pôvodný priečinok iba na čítanie a zostaví samostatný `dist/data.js`. Obnova: `node HTML5/tools/extract-data.cjs "C:/Users/bumbe/Downloads/rea-savetheworld-report"` z koreňa projektu. Browser nepotrebuje pôvodné súbory ani internet.

## Prenesené dôkazy

- Všetkých 4 907 dekódovaných objektov je v prehľadávateľnom `REPORT_DATA.catalog`. Každý má kategóriu, meno, pôvodné pathId a dáta. Unity referencie sú skrátené na `@fileID:pathID` alebo `guid:…`; nejde o export samotných modelov/textúr. Odkazy na GameObject/Script a príznak m_Enabled sú v kompaktnom katalógu vynechané. Desatinné čísla sú zaokrúhlené na šesť miest.
- `planets`: desať hlavných planét, pôvodné indexy, polomery, gravitácia, zoznamy zdrojov a stropy vybavenia. Varianty, eventy a výzvy zostávajú v katalógu.
- `tools`, `armor`, `resources`, `enemies`, `recipes`, `refineryUpgrades`, `movements`, `prestige`: odvodené pohľady priamo zo serializovaných záznamov. Číselné hodnoty nie sú spätne domyslené.
- `settings` obsahuje celý dodaný konfiguračný snapshot; vnorené JSON reťazce sú rozbalené. `quests` prenáša mená a konfiguračné definície úloh. Skratkové polia konfigurácie nemajú potvrdenú runtime sémantiku.
- `provenance.sourceSha256` identifikuje vstupný decoded-scriptables.json. Pôvodná hra bola vo verzii 2.9.1, Unity 2022.3.19f1.

## Oprava chybného mapovania enumov

`enum-values.json` má zjavne nesprávne hodnoty: napr. Wood = 134611970, hoci konkrétne ResourceData záznamy ukladajú Wood ako type = 1. Zdrojový summarize.cjs číta všetky enumy cez readInt32LE bez ohľadu na ich podkladový typ, čo pri byte enumoch spája susedné hodnoty. Tento HTML5 export tieto enum hodnoty nepoužíva. `resourceNames` sa odvodzuje z dvojice type/title priamo v ResourceData; neznáma hodnota zostáva explicitne označená. Typy atribútov sa v exporte ponechávajú ako číselné identifikátory, ich herné použitie je interpretácia.

## Čo nie je pôvodný herný kód

Report nemá dekompilovanú IL2CPP implementáciu ani spustiteľný Unity projekt. HTML5 hra preto musí doplniť vlastný renderer, mapy, ovládanie, kolízie, boj, interakcie, ukladanie, rozmiestnenie surovín a nepriateľov, odomykanie portálov, questové podmienky a vyváženie. Tieto systémy sú rekonštrukcia. Recepty môžu mať samostatné refiningDuration a tier refineDuration; export zachováva oba údaje. Pri zjednodušení receptov môže herná vrstva zvoliť prvý tier. Dátový katalóg nezaručuje, že každý záznam je aktívna herná mechanika.

Pôvodné reklamy, billing, serverové služby, eventy a platené odmeny nepredstavujú funkčný backend tohto buildu. Lokálna hra nesmie prezentovať vlastné vyváženie ako potvrdenú pôvodnú logiku.

## Pravidlá použité v hrateľnom builde

- Priamo použité: názvy a poradie desiatich planét, planetárne stropy nástrojov/brnenia, prvý atribút nástrojov na úrovniach 0–73 a dva domovské recepty dosiek/ocele vrátane vstupov, výstupov a času.
- Pohyb používa reportované rýchlosti 4 / 3,5 / 3 a damping 15. Projekcia do 2D, kolízia s okrajom ostrova a aplikácia rýchlosti sú vlastná implementácia. Polomery a gravitácia z reportu ostávajú v archíve; renderer používa jednotnú veľkosť ostrova.
- Vlastné vyváženie: odolnosť a výnos uzlov, ich 14-sekundová obnova, zdravie/útok nepriateľov, odmeny, ceny vylepšení a obnovy planét, porážka, rozvoj domova, pomocníci, questové ciele, prestige reset a 10 % bonus. Prvý atribút nástroja interpretujeme ako silu; report jeho presnú runtime sémantiku nepotvrdzuje.
- Základné drevo a kameň sú dostupné na každej planéte, aj keď ich pôvodný planetárny zoznam neuvádza. Rozmiestnenie a doplnkové zdroje sú zjednodušené; nezavádzame všetky pôvodné typy a minimálne úrovne zdrojov. Brnenie tvorí jedna zjednodušená úroveň namiesto dvoch slotov.
- Rafinérie majú vlastnú trojmiestnu frontu. Upgrade využíva reportovanú hodnotu 1,75 %, ale zvolený násobný výpočet, limit 20 úrovní a cena iba v minciach sú vlastné. Pôvodné tickety a monetizácia sa neuplatňujú.
- Výroba sa dokončuje podľa reálneho času aj pri pauze. Offline výroba pomocníkov je vlastná, obmedzená na hodinu. Pôvodná konfigurácia offline systémov sa neinterpretuje ako potvrdený runtime mechanizmus.
