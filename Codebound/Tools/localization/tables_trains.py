# Rows for build_tables.py (exec'd by it): industrial simulation Etapa 7 — trains.
# Order: en, pt-PT, pt-BR, es, fr, de, it, ja, zh-Hans, ko.
_W = [("1", "1"), ("2", "2"), ("3", "3"), ("4", "4")]
T10 = {
"building.rail": ["Rail", "Carril", "Trilho", "Raíl", "Rail", "Schiene", "Binario", "レール", "铁轨", "레일"],
"building.rail_signal": ["Rail Signal", "Sinal de via", "Sinal ferroviário", "Señal de vía", "Signal ferroviaire", "Eisenbahnsignal", "Segnale ferroviario", "鉄道信号", "铁路信号", "철도 신호"],
"building.train_stop": ["Train Stop", "Paragem de comboio", "Parada de trem", "Parada de tren", "Arrêt de train", "Bahnhof", "Fermata del treno", "駅", "火车站", "기차역"],
"building.train_depot": ["Train Depot", "Depósito de comboios", "Garagem de trens", "Depósito de trenes", "Dépôt de trains", "Zugdepot", "Deposito treni", "車両基地", "列车库", "열차 차고"],
"item.locomotive": ["Locomotive", "Locomotiva", "Locomotiva", "Locomotora", "Locomotive", "Lokomotive", "Locomotiva", "機関車", "机车", "기관차"],
"item.cargo_wagon": ["Cargo Wagon", "Vagão de carga", "Vagão de carga", "Vagón de carga", "Wagon de marchandises", "Güterwagen", "Vagone merci", "貨車", "货运车厢", "화물 차량"],
"tech.railways": ["Railways", "Caminhos de ferro", "Ferrovias", "Ferrocarriles", "Chemins de fer", "Eisenbahn", "Ferrovie", "鉄道", "铁路", "철도"],
"mp.train": ["Train: {0} · {1} wagons · {2} coal", "Comboio: {0} · {1} vagões · {2} carvão", "Trem: {0} · {1} vagões · {2} carvão", "Tren: {0} · {1} vagones · {2} carbón", "Train : {0} · {1} wagons · {2} charbon", "Zug: {0} · {1} Wagen · {2} Kohle", "Treno: {0} · {1} vagoni · {2} carbone", "列車: {0} · 貨車 {1} · 石炭 {2}", "列车：{0} · {1} 节车厢 · {2} 煤", "열차: {0} · 화물칸 {1} · 석탄 {2}"],
"mp.schedule": ["Schedule, e.g. iron_mine:full,base:empty,depot:30 (full, empty, seconds or iron_ore>=100)", "Horário, p. ex. iron_mine:full,base:empty,depot:30 (full, empty, segundos ou iron_ore>=100)", "Horário, ex.: iron_mine:full,base:empty,depot:30 (full, empty, segundos ou iron_ore>=100)", "Horario, p. ej. iron_mine:full,base:empty,depot:30 (full, empty, segundos o iron_ore>=100)", "Horaire, ex. iron_mine:full,base:empty,depot:30 (full, empty, secondes ou iron_ore>=100)", "Fahrplan, z. B. iron_mine:full,base:empty,depot:30 (full, empty, Sekunden oder iron_ore>=100)", "Orario, es. iron_mine:full,base:empty,depot:30 (full, empty, secondi o iron_ore>=100)", "ダイヤ 例: iron_mine:full,base:empty,depot:30（full・empty・秒・iron_ore>=100）", "时刻表，例如 iron_mine:full,base:empty,depot:30（full、empty、秒数或 iron_ore>=100）", "시간표, 예: iron_mine:full,base:empty,depot:30 (full, empty, 초 또는 iron_ore>=100)"],
"mp.stopName": ["Stop name (a-z, 0-9, _), e.g. iron_mine", "Nome da paragem (a-z, 0-9, _), p. ex. iron_mine", "Nome da parada (a-z, 0-9, _), ex.: iron_mine", "Nombre de la parada (a-z, 0-9, _), p. ej. iron_mine", "Nom de l'arrêt (a-z, 0-9, _), ex. iron_mine", "Name des Bahnhofs (a-z, 0-9, _), z. B. iron_mine", "Nome della fermata (a-z, 0-9, _), es. iron_mine", "駅名（a-z, 0-9, _）例: iron_mine", "车站名（a-z、0-9、_），例如 iron_mine", "역 이름 (a-z, 0-9, _), 예: iron_mine"],
"mp.role.garage": ["train", "comboio", "trem", "tren", "train", "Zug", "treno", "列車", "列车", "열차"],
"st.no_locomotive": ["no locomotive", "sem locomotiva", "sem locomotiva", "sin locomotora", "pas de locomotive", "keine Lok", "nessuna locomotiva", "機関車なし", "无机车", "기관차 없음"],
"st.no_schedule": ["no schedule", "sem horário", "sem horário", "sin horario", "pas d'horaire", "kein Fahrplan", "nessun orario", "ダイヤなし", "无时刻表", "시간표 없음"],
"st.waiting_at_signal": ["waiting at signal", "à espera no sinal", "esperando no sinal", "esperando en la señal", "attend au signal", "wartet am Signal", "in attesa al segnale", "信号待ち", "等待信号", "신호 대기"],
"st.no_rail_in_front": ["no rail in front", "sem carril à frente", "sem trilho à frente", "sin raíl delante", "pas de rail devant", "keine Schiene davor", "nessun binario davanti", "前にレールなし", "前方无铁轨", "앞에 레일 없음"],
"st.rail": ["rail", "carril", "trilho", "raíl", "rail", "Schiene", "binario", "レール", "铁轨", "레일"],
"st.green": ["green", "verde", "verde", "verde", "vert", "grün", "verde", "青", "绿", "초록"],
"st.no_name": ["no name", "sem nome", "sem nome", "sin nombre", "sans nom", "kein Name", "senza nome", "名前なし", "未命名", "이름 없음"],
}
for _n in "1234":
    T10["mp.role.wagon" + _n] = ["wagon " + _n, "vagão " + _n, "vagão " + _n, "vagón " + _n, "wagon " + _n, "Wagen " + _n, "vagone " + _n, "貨車 " + _n, "车厢 " + _n, "화물칸 " + _n]
