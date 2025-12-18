using Learn_Earn.Models;
using Microsoft.EntityFrameworkCore;

namespace Learn_Earn.Data
{
    public static class SeedData
    {
        public static async Task EnsureSeedDataAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure DB is created (migrations should already be applied)
            await db.Database.EnsureCreatedAsync();

            // Dacă avem doar lecțiile demo vechi, le înlocuim cu unele reale
            var existingLessons = await db.Lessons.ToListAsync();
            if (existingLessons.Any() && existingLessons.All(l => l.Title.StartsWith("Lecția demo")))
            {
                db.Lessons.RemoveRange(existingLessons);
                await db.SaveChangesAsync();
            }

            // Dacă există deja lecții reale, nu mai facem nimic
            if (await db.Lessons.AnyAsync())
                return;

            var now = DateTime.UtcNow;

            var lessons = new List<Lesson>
            {
                // ENGLISH (5 lessons)
                new Lesson
                {
                    Title = "English A1 – Present Simple (daily routines)",
                    Language = "English",
                    Difficulty = "Easy",
                    DurationMinutes = 20,
                    Content =
@"In this lesson you learn the Present Simple tense and how it is used to describe habits and routines.

1. Form
- With I/you/we/they we use the base form of the verb: I work, they study, we live.
- With he/she/it we add -s or -es to the verb: she works, he studies, it goes.

2. Uses of Present Simple
- Daily routines and repeated actions: I get up at 7 a.m., She goes to school every day.
- Facts and general truths: Water boils at 100 degrees, The earth goes around the sun.
- Timetables and schedules: The bus leaves at 8:15, The lesson starts at 9:00.

3. Adverbs of frequency
- always, usually, often, sometimes, rarely, never.
These are often placed before the main verb: I usually get up at 7. She never drinks coffee.

4. Negative and questions
- Negative: do/does + not + verb: I do not (don’t) work, He does not (doesn’t) like tea.
- Questions: Do/Does + subject + verb?: Do you work here? Does she live in London?

Homework:
1. Write 5 sentences about your morning routine using Present Simple and an adverb of frequency.
2. Transform those 5 sentences for he/she (""I get up at 7."" → ""He gets up at 7."").",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "English A1 – To be + personal information",
                    Language = "English",
                    Difficulty = "Easy",
                    DurationMinutes = 20,
                    Content =
@"In this lesson you review the verb ""to be"" in the present tense and use it with basic personal information.

1. Forms of ""to be""
- I am
- you are
- he/she/it is
- we are
- they are

2. Uses of ""to be""
- Identity: I am a student. She is a teacher.
- Origin: I am from Romania. They are from Spain.
- Feelings and states: I am tired. We are happy.
- Position: The book is on the table. The classroom is upstairs.

3. Questions and short answers
- Are you a teacher? Yes, I am. / No, I am not.
- Is he from Germany? Yes, he is. / No, he isn’t.

Homework:
Write a short self‑introduction (5–7 sentences) using ""to be"" to say who you are, where you are from, what you do and how you feel today.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "English A2 – Parts of Speech overview",
                    Language = "English",
                    Difficulty = "Easy",
                    DurationMinutes = 25,
                    Content =
@"In this lesson you review the main parts of speech in English and understand their roles in a sentence.

1. Nouns
Words that name people, places, things or ideas: student, city, book, freedom.

2. Verbs
Words that express actions or states: run, think, be, feel, have.

3. Adjectives
Words that describe nouns: big, interesting, blue, difficult.

4. Adverbs
Words that describe verbs, adjectives or other adverbs: quickly, very, always, quite.

5. Pronouns
Words that replace nouns to avoid repetition: I, you, he, she, it, we, they, this, that.

6. Prepositions
Words that show the relationship between elements in the sentence: in, on, at, under, between, next to.

7. Conjunctions
Words that connect words, phrases or clauses: and, but, or, because, although.

Example sentence:
""The young teacher speaks very clearly in class.""
- Noun: teacher, class
- Adjective: young
- Verb: speaks
- Adverb: clearly

Homework:
Create 5 English sentences of your own. Underline the nouns, circle the verbs and highlight the adjectives.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "English B1 – Present Continuous vs Present Simple",
                    Language = "English",
                    Difficulty = "Medium",
                    DurationMinutes = 30,
                    Content =
@"In this lesson you compare Present Simple and Present Continuous and learn when to use each tense.

1. Present Simple – form and use
- Form: subject + base verb / he-she-it + verb + s/es.
- Use: habits and routines (I work every day), permanent situations (She lives in Berlin), general truths (Water boils at 100°C).

2. Present Continuous – form and use
- Form: am/are/is + verb + -ing: I am working, She is studying, They are playing.
- Use: actions happening now (I am reading this text), temporary actions (She is staying with friends this week), changing situations (The weather is getting warmer).

3. Typical signal words
- Present Simple: always, often, sometimes, usually, every day, on Mondays.
- Present Continuous: now, at the moment, today, this week, right now.

4. Contrast examples
- I usually walk to school, but today I am taking the bus.
- She works in an office, but this month she is working from home.

Homework:
Write 8 sentences about your weekly life: 4 in Present Simple (habits and routines) and 4 in Present Continuous (things happening now or this week).",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "English B1 – Past Simple (regular verbs)",
                    Language = "English",
                    Difficulty = "Medium",
                    DurationMinutes = 30,
                    Content =
@"In this lesson you learn the Past Simple tense with regular verbs and when to use it.

1. Form of Past Simple (regular verbs)
- Base verb + -ed: work → worked, play → played, visit → visited.
- Spelling rules: verbs ending in -e add only -d (live → lived); verbs ending consonant + y change y to ied (study → studied).

2. Use of Past Simple
- Finished actions in the past with a clear time reference: yesterday, last week, in 2020.
Examples:
""I visited my grandparents last weekend.""
""She studied for the test yesterday evening.""

3. Negative and questions
- Negative: did not (didn’t) + base verb: I didn’t watch TV last night.
- Questions: Did + subject + base verb?: Did you go to school yesterday?

4. Timeline idea
Past Simple talks about a point or period that is completely finished and not connected to now.

Homework:
Write a short diary entry (8–10 sentences) about what you did last weekend using Past Simple with regular verbs. Include at least three time expressions (yesterday, last Saturday, in the afternoon, etc.).",
                    CreatedAt = now
                },

                // GERMAN (5 lessons)
                new Lesson
                {
                    Title = "German A1 – Präsens (regelmäßige Verben)",
                    Language = "German",
                    Difficulty = "Easy",
                    DurationMinutes = 20,
                    Content =
@"In dieser Lektion lernst du das Präsens mit regelmäßigen Verben im Deutschen.

1. Bildung des Präsens (regelmäßige Verben)
Beispielverb: lernen (to learn)
ich lerne
du lernst
er/sie/es lernt
wir lernen
ihr lernt
sie/Sie lernen

Die Endungen sind: -e, -st, -t, -en, -t, -en.

2. Verwendung des Präsens
- für Handlungen in der Gegenwart: Ich lerne jetzt Deutsch.
- für regelmäßige Handlungen: Ich lerne jeden Tag eine Stunde.
- für allgemeine Aussagen: Wasser kocht bei 100 Grad.

3. Satzstellung im Aussagesatz
Subjekt + konjugiertes Verb + Rest:
""Ich lerne Deutsch in der Schule.""

4. Fragen im Präsens
Verb + Subjekt + Rest:
""Lernst du Deutsch?""

Homework:
Konjugiere die Verben „machen, spielen, arbeiten“ im Präsens und schreibe für jedes Verb zwei Beispielsätze über deinen Alltag.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "German A1 – Personalpronomen und Verb sein",
                    Language = "German",
                    Difficulty = "Easy",
                    DurationMinutes = 20,
                    Content =
@"In dieser Lektion wiederholst du die Personalpronomen und das Verb ""sein"" im Präsens.

1. Personalpronomen
ich, du, er, sie, es, wir, ihr, sie, Sie.

2. Verb ""sein"" im Präsens
ich bin
du bist
er/sie/es ist
wir sind
ihr seid
sie/Sie sind

3. Verwendung von ""sein""
- zur Beschreibung von Personen: Ich bin Schüler. Sie ist Lehrerin.
- zur Angabe von Herkunft: Ich bin aus Rumänien. Wir sind aus Deutschland.
- zur Beschreibung von Zuständen: Ich bin müde. Wir sind glücklich.

4. Fragesätze
""Bist du müde?"" – ""Ja, ich bin müde."" / ""Nein, ich bin nicht müde.""

Homework:
Schreibe 6 kurze Sätze über dich und deine Freunde mit ""sein"" (z. B. Name, Alter, Herkunft, Charaktereigenschaften).",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "German A2 – Wortarten (Nomen, Verben, Adjektive)",
                    Language = "German",
                    Difficulty = "Medium",
                    DurationMinutes = 25,
                    Content =
@"In dieser Lektion wiederholst du die wichtigsten Wortarten im Deutschen: Nomen, Verben und Adjektive.

1. Nomen (Substantive)
- werden großgeschrieben: der Lehrer, die Stadt, das Auto.
- bezeichnen Personen, Dinge, Orte, Ideen.

2. Verben
- bezeichnen Handlungen oder Zustände: gehen, sein, haben, lernen.
- werden im Satz konjugiert: ich gehe, du gehst, er geht.

3. Adjektive
- beschreiben Nomen: schnell, laut, freundlich, interessant.
- stehen oft vor dem Nomen: der freundliche Lehrer, die interessante Übung.

4. Beispielsatz
""Der freundliche Lehrer erklärt die neue Grammatik.""
Nomen: Lehrer, Grammatik
Verb: erklärt
Adjektiv: freundliche

Homework:
Schreibe 5 eigene Sätze und markiere alle Nomen, Verben und Adjektive jeweils in einer anderen Farbe oder mit unterschiedlichen Zeichen.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "German A2 – Trennbare Verben im Präsens",
                    Language = "German",
                    Difficulty = "Medium",
                    DurationMinutes = 25,
                    Content =
@"In dieser Lektion lernst du trennbare Verben im Präsens kennen und verwendest sie in Alltagssätzen.

1. Was sind trennbare Verben?
Trennbare Verben bestehen aus einem Präfix und einem Verb: aufstehen, einkaufen, anrufen, mitkommen.

2. Stellung im Aussagesatz
Das konjugierte Verb steht an Position 2, die Präfix-Silbe am Ende:
""Ich stehe um 7 Uhr auf.""
""Wir kaufen heute im Supermarkt ein.""

3. Stellung in Fragen ohne Fragewort
Verb an Position 1, Präfix am Ende:
""Stehst du früh auf?""
""Kauft ihr morgen ein?""

4. Bedeutung
Das Präfix verändert oft die Bedeutung des Verbs:
- stehen → aufstehen (to get up)
- rufen → anrufen (to call)

Homework:
Schreibe 6 Sätze mit verschiedenen trennbaren Verben (z. B. aufstehen, einkaufen, anrufen, mitkommen) über deinen Alltag.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "German B1 – Perfekt mit haben",
                    Language = "German",
                    Difficulty = "Medium",
                    DurationMinutes = 30,
                    Content =
@"In dieser Lektion lernst du das Perfekt mit dem Hilfsverb „haben“ – eine häufige Vergangenheitsform in der gesprochenen Sprache.

1. Bildung des Perfekts
Perfekt = haben (konjugiert) + Partizip II des Verbs.
Beispiele: ich habe gelernt, du hast gespielt, wir haben gearbeitet.

2. Verwendung des Perfekts
- für abgeschlossene Handlungen in der Vergangenheit, besonders in der gesprochenen Sprache:
""Gestern habe ich lange gearbeitet.""
""Am Wochenende haben wir Freunde besucht.""

3. Stellung im Satz
Das konjugierte Verb (haben) steht an Position 2, das Partizip II am Satzende.

4. Typische Zeitangaben
gestern, letzte Woche, am Wochenende, vor zwei Tagen.

Homework:
Schreibe einen kurzen Text (8–10 Sätze) über dein letztes Wochenende im Perfekt. Nutze mindestens drei verschiedene Verben mit ""haben"".",
                    CreatedAt = now
                },

                // ROMANIAN (5 lessons)
                new Lesson
                {
                    Title = "Română – Părțile de vorbire de bază",
                    Language = "Romanian",
                    Difficulty = "Easy",
                    DurationMinutes = 30,
                    Content =
@"În această lecție recapitulezi părțile de vorbire de bază în limba română și rolul lor în propoziție.

1. Substantivul
Numește ființe, lucruri, locuri, idei: elev, carte, oraș, libertate.
Are gen (masculin, feminin, neutru) și număr (singular, plural).

2. Verbul
Exprimă acțiuni sau stări: a citi, a alerga, a fi, a avea.
Se conjugă după persoană, număr, timp, mod.

3. Adjectivul
Arată o însușire a unui substantiv: frumos, harnic, interesant.
Se acordă cu substantivul în gen și număr: elev harnic / elevi harnici.

4. Adverbul
Determină un verb, un adjectiv sau alt adverb: repede, foarte, bine.
Exemplu: aleargă repede, foarte frumos.

5. Pronumele
Ține locul unui substantiv: eu, tu, el, ea, noi, voi, ei, ele.

Exemplu de propoziție:
„Eleva harnică lucrează foarte serios.”
Substantiv: eleva
Adjectiv: harnică
Verb: lucrează
Adverb: foarte, serios

Homework:
Scrie 5 propoziții proprii și subliniază substantivele, încercuind verbele și încadrând adjectivul în paranteze pătrate [].",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "Română – Timpul prezent al verbului",
                    Language = "Romanian",
                    Difficulty = "Easy",
                    DurationMinutes = 20,
                    Content =
@"În această lecție studiezi timpul prezent al verbului în limba română.

1. Valoarea timpului prezent
Prezentul exprimă acțiuni care se petrec acum sau în mod obișnuit, repetat:
„Eu citesc acum.”, „În fiecare dimineață beau lapte.”.

2. Conjugarea verbului „a învăța”
eu învăț
tu înveți
el/ea învață
noi învățăm
voi învățați
ei/ele învață

3. Prezentul și obiceiurile
Se folosește frecvent cu adverbe de timp: mereu, de obicei, zilnic, uneori.
Exemplu: „De obicei, eu învăț seara.”.

4. Neagarea
Se formează cu „nu”: „Nu învăț acum.”, „Nu joc fotbal în fiecare zi.”.

Homework:
Conjugă verbele „a citi, a scrie, a juca” la timpul prezent pentru toate persoanele și scrie 5 propoziții la prezent despre activitățile tale zilnice.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "Română – Substantivul: gen și număr",
                    Language = "Romanian",
                    Difficulty = "Medium",
                    DurationMinutes = 25,
                    Content =
@"În această lecție reiei noțiunile de gen și număr ale substantivului în limba română.

1. Genul substantivului
- masculin: elev, băiat, caiet;
- feminin: fată, carte, floare;
- neutru: manual, scaun, pix.
Substantivele neutre se comportă ca masculine la singular și ca feminine la plural.

2. Numărul substantivului
- singular: indică un singur obiect sau ființă: elev, fată, manual;
- plural: indică mai multe: elevi, fete, manuale.

3. Exemple
elev / elevi, fată / fete, manual / manuale, copil / copii.

4. Importanța genului și numărului
Genul și numărul substantivului influențează forma articolului și a adjectivului:
„elevul harnic / elevii harnici”, „fata isteață / fetele istețe”.

Homework:
Alege 10 substantive din jurul tău (lucruri din cameră, persoane, obiecte școlare) și notează pentru fiecare: forma de singular, forma de plural și genul.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "Română – Adjectivul și acordul cu substantivul",
                    Language = "Romanian",
                    Difficulty = "Medium",
                    DurationMinutes = 25,
                    Content =
@"În această lecție studiezi adjectivul și acordul lui cu substantivul.

1. Rolul adjectivului
Adjectivul exprimă o însușire a substantivului: „carte interesantă”, „elev serios”.

2. Acordul adjectivului
Adjectivul se acordă cu substantivul determinat în gen și număr:
- singular masculin: băiat isteț;
- plural masculin: băieți isteți;
- singular feminin: fată cuminte;
- plural feminin: fete cuminți.

3. Poziția adjectivului
În mod obișnuit stă după substantiv: „copil curios”, „temă grea”, dar poate apărea și înainte: „frumoasa floare”.

4. Importanța acordului
Lipsa acordului corect poate duce la exprimări greșite: „fete cuminți”, nu „fete cuminte”.

Homework:
Scrie 6 propoziții la singular cu câte un adjectiv lângă substantiv și apoi transformă-le la plural, având grijă să faci acordul corect.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "Română – Prezentul verbelor neregulate (a fi, a avea)",
                    Language = "Romanian",
                    Difficulty = "Medium",
                    DurationMinutes = 20,
                    Content =
@"În această lecție lucrezi cu două verbe neregulate foarte importante: „a fi” și „a avea”.

1. Verbul „a fi” la prezent
sunt, ești, este, suntem, sunteți, sunt.
Exemple: „Eu sunt elev.”, „Tu ești obosit.”, „Noi suntem prieteni.”.

2. Verbul „a avea” la prezent
am, ai, are, avem, aveți, au.
Exemple: „Eu am trei caiete.”, „Ei au un test mâine.”.

3. Rolul lor în propoziție
„a fi” – verb copulativ, leagă subiectul de nume predicativ:
„Maria este fericită.”.
„a avea” – exprimă posesia, dar și expresii fixe: „a avea grijă”, „a avea dreptate”.

Homework:
Scrie 8 propoziții în care folosești forme diferite ale verbelor „a fi” și „a avea”, încercând să combini și cu alte părți de vorbire (adjective, substantive).",
                    CreatedAt = now
                },

                // SPANISH (5 lessons)
                new Lesson
                {
                    Title = "Spanish A1 – Presento mi rutina diaria",
                    Language = "Spanish",
                    Difficulty = "Easy",
                    DurationMinutes = 20,
                    Content =
@"En esta lección aprendes a usar el presente de los verbos regulares para describir tu rutina diaria.

1. Verbos regulares en -ar, -er, -ir
Ejemplos: hablar, comer, vivir, estudiar.
En presente, las terminaciones cambian según la persona:
hablo, hablas, habla, hablamos, habláis, hablan.

2. Rutina diaria
Usamos el presente para hablar de acciones habituales:
""Me levanto a las siete, desayuno, voy a la escuela y estudio inglés por la tarde.""

3. Marcadores de frecuencia
siempre, normalmente, a veces, nunca.
Ejemplo: ""Normalmente estudio una hora al día, pero a veces estudio más.""

Homework:
Escribe 6 frases sobre tu rutina de la mañana usando el presente y al menos dos marcadores de frecuencia.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "Spanish A1 – Ser y estar: información básica",
                    Language = "Spanish",
                    Difficulty = "Easy",
                    DurationMinutes = 20,
                    Content =
@"En esta lección repasas la diferencia entre los verbos ""ser"" y ""estar"" en español.

1. Usos principales de ""ser""
- identidad y profesión: ""Soy estudiante"", ""Ella es médica.""
- características permanentes: ""Es alto"", ""Somos responsables.""
- origen: ""Soy de Rumanía.""

2. Usos principales de ""estar""
- estados físicos y emocionales: ""Estoy cansado"", ""Estamos contentos.""
- localización: ""Estamos en clase.""
- acciones en progreso (con gerundio): ""Estoy estudiando español.""

3. Comparación sencilla
""ser"" = qué eres (características estables);
""estar"" = cómo estás o dónde estás (algo más temporal).

Homework:
Escribe 8 frases sobre ti y tus amigos: 4 usando ""ser"" (quiénes sois, de dónde sois, cómo sois) y 4 usando ""estar"" (cómo os sentís hoy, dónde estáis ahora).",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "Spanish A2 – Sustantivos y adjetivos",
                    Language = "Spanish",
                    Difficulty = "Medium",
                    DurationMinutes = 25,
                    Content =
@"En esta lección trabajas la concordancia entre sustantivos y adjetivos en género y número.

1. Género: masculino y femenino
Ejemplos: chico alto, chica simpática.

2. Número: singular y plural
chico alto → chicos altos;
chica simpática → chicas simpáticas.

3. Reglas básicas
El adjetivo debe coincidir con el sustantivo:
- en masculino/femenino;
- en singular/plural.

Homework:
Transforma 6 grupos nominales del singular al plural, manteniendo la concordancia correcta entre sustantivo y adjetivo, y escribe 4 frases completas donde uses esas combinaciones.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "Spanish A2 – Presente continuo",
                    Language = "Spanish",
                    Difficulty = "Medium",
                    DurationMinutes = 25,
                    Content =
@"En esta lección estudias el presente continuo en español para hablar de acciones que ocurren ahora.

1. Forma del presente continuo
estar (conjugado) + gerundio:
estoy estudiando, estás leyendo, está escribiendo, estamos jugando, estáis trabajando, están comiendo.

2. Uso principal
Acciones en progreso en el momento de hablar:
""Ahora estoy haciendo mis deberes."", ""Mis padres están viendo la televisión.""

3. Marcadores
ahora, en este momento, hoy, esta semana.

Homework:
Escribe 6 frases explicando qué estás haciendo tú y tu familia en este momento o durante este día, usando el presente continuo y al menos un marcador temporal en cada frase.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "Spanish B1 – Pretérito indefinido (regulares)",
                    Language = "Spanish",
                    Difficulty = "Medium",
                    DurationMinutes = 30,
                    Content =
@"En esta lección aprendes el pretérito indefinido con verbos regulares y su uso básico.

1. Conjugación regular
-ar: hablé, hablaste, habló, hablamos, hablasteis, hablaron.
-er: comí, comiste, comió, comimos, comisteis, comieron.
-ir: viví, viviste, vivió, vivimos, vivisteis, vivieron.

2. Uso
Acciones terminadas en un momento concreto del pasado:
""Ayer estudié español."", ""El año pasado viajamos a Madrid.""

3. Marcadores temporales
ayer, anoche, el lunes pasado, en 2020, hace dos días.

Homework:
Escribe un párrafo de 8–10 frases sobre lo que hiciste el fin de semana pasado usando el pretérito indefinido y al menos tres marcadores temporales.",
                    CreatedAt = now
                },

                // FRENCH (5 lessons)
                new Lesson
                {
                    Title = "French A1 – Se présenter au présent",
                    Language = "French",
                    Difficulty = "Easy",
                    DurationMinutes = 20,
                    Content =
@"Dans cette leçon tu apprends à te présenter en utilisant le verbe ""être"" et quelques adjectifs simples.

1. Le verbe ""être"" au présent
je suis, tu es, il/elle est, nous sommes, vous êtes, ils/elles sont.

2. Se présenter
Exemples:
""Je suis élève."", ""Je suis roumain(e)."", ""Je suis motivé(e)."".
Tu peux aussi dire ton âge: ""J’ai quatorze ans.""

3. Adjectifs fréquents
grand(e), petit(e), timide, sociable, sérieux/sérieuse, content(e).

Homework:
Écris une courte présentation (6–8 phrases) avec ton nom, ton âge, ta ville, ta nationalité et deux ou trois adjectifs qui te décrivent.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "French A1 – Le présent des verbes en -er",
                    Language = "French",
                    Difficulty = "Easy",
                    DurationMinutes = 20,
                    Content =
@"Dans cette leçon tu conjugues des verbes réguliers en -er au présent pour parler de ce que tu aimes faire.

1. Modèle de conjugaison
parler: je parle, tu parles, il/elle parle, nous parlons, vous parlez, ils/elles parlent.

2. Utilisation
Tu peux décrire tes loisirs et activités:
""J’aime regarder des films."", ""Je joue au foot."", ""Je parle avec mes amis.""

Homework:
Écris 6 phrases avec des verbes en -er sur tes hobbies et tes activités quotidiennes (parler, aimer, regarder, écouter, jouer…).",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "French A2 – Articles définis et indéfinis",
                    Language = "French",
                    Difficulty = "Medium",
                    DurationMinutes = 25,
                    Content =
@"Dans cette leçon tu révises les articles définis et indéfinis en français.

1. Articles définis
le (masculin singulier), la (féminin singulier), les (pluriel).
Exemples: le livre, la maison, les amis.

2. Articles indéfinis
un (masculin singulier), une (féminin singulier), des (pluriel).
Exemples: un élève, une fille, des cahiers.

3. Choisir l’article
On utilise l’article défini pour parler de quelque chose de précis, connu;
l’article indéfini pour quelque chose de non précisé.

Homework:
Complète 10 groupes nominaux en choisissant l’article correct (le/la/les ou un/une/des) puis écris 5 phrases en utilisant ces groupes nominaux.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "French A2 – Adjectifs et accord",
                    Language = "French",
                    Difficulty = "Medium",
                    DurationMinutes = 25,
                    Content =
@"Dans cette leçon tu travailles l’accord des adjectifs avec le nom en genre et en nombre.

1. Accord de base
L’adjectif change selon le nom:
un livre intéressant / des livres intéressants,
une maison blanche / des maisons blanches.

2. Féminin et pluriel
Souvent on ajoute -e au féminin et -s au pluriel, mais il y a des irrégularités.

3. Importance de l’accord
Un mauvais accord rend la phrase incorrecte en français écrit:
on dit ""des maisons blanches"", pas ""des maisons blanc"".

Homework:
Transforme 6 phrases du singulier au pluriel en respectant l’accord entre le nom et l’adjectif, puis souligne les adjectifs dans chaque phrase.",
                    CreatedAt = now
                },
                new Lesson
                {
                    Title = "French B1 – Passé composé avec avoir",
                    Language = "French",
                    Difficulty = "Medium",
                    DurationMinutes = 30,
                    Content =
@"Dans cette leçon tu apprends le passé composé avec l’auxiliaire avoir, très utilisé pour parler du passé.

1. Formation
Passé composé = avoir (conjugué) + participe passé du verbe.
Exemples: j’ai étudié, tu as joué, nous avons regardé un film.

2. Utilisation
On l’emploie pour des actions terminées à un moment précis du passé:
""Hier, j’ai fini mes devoirs."", ""Le week-end dernier, nous avons visité nos grands-parents.""

3. Marqueurs temporels
hier, la semaine dernière, l’année dernière, il y a deux jours.

Homework:
Écris un court texte (8–10 phrases) sur ta journée d’hier en utilisant le passé composé avec avoir et au moins trois expressions de temps.",
                    CreatedAt = now
                }
            };

            // Add an extra theoretical and study-skills section to every lesson
            foreach (var lesson in lessons)
            {
                lesson.Content +=
                    "\n\n---\nADDITIONAL THEORY & STUDY TIPS\n" +
                    "\n1. Connect form and meaning\n" +
                    "Always link the grammatical form (endings, word order, auxiliaries) with its meaning in real situations. " +
                    "Ask yourself: WHEN do I use this tense or structure and WHAT exactly does it say about time, frequency or attitude?\n" +
                    "\n2. Notice patterns in examples\n" +
                    "Look at several correct example sentences and underline the repeated patterns (positions of the verb, subject, adverbs, negatives). " +
                    "Try to produce your own examples by copying these patterns with different words.\n" +
                    "\n3. Compare with your own language\n" +
                    "Think how the same idea is expressed in your first language. Are tenses the same, earlier, or completely different? " +
                    "This comparison helps you predict typical mistakes and avoid them.\n" +
                    "\n4. Build a mini reference sheet\n" +
                    "For each topic, write on one page: form (tables), use (when), 3–4 signal words and 5 model sentences you really understand. " +
                    "Keep this sheet near you when doing homework and gradually update it.\n" +
                    "\n5. Active practice\n" +
                    "Try to speak or write short texts using the target structures from the lesson (not only isolated sentences). " +
                    "The more you actively use the grammar in context, the faster it becomes automatic.";
            }

            db.Lessons.AddRange(lessons);
            await db.SaveChangesAsync();
        }
    }
}
