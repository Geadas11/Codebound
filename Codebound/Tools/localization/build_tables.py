# Generates Assets/StreamingAssets/Localization/<lang>.json from the table below.
# Order of every row: en, pt-PT, pt-BR, es, fr, de, it, ja, zh-Hans, ko
# Run: Codebound/Tools/python/python.exe -I Codebound/Tools/localization/build_tables.py
import json, os

LANGS = ["en", "pt-PT", "pt-BR", "es", "fr", "de", "it", "ja", "zh-Hans", "ko"]

T = {
"menu.title": ["PAUSED — Options", "EM PAUSA — Opções", "PAUSADO — Opções", "EN PAUSA — Opciones", "PAUSE — Options", "PAUSIERT — Optionen", "IN PAUSA — Opzioni", "一時停止 — オプション", "已暂停 — 选项", "일시 정지 — 옵션"],
"menu.help": ["Up/Down select · Left/Right or Enter change · Esc back · mouse works too", "Cima/Baixo escolher · Esquerda/Direita ou Enter mudar · Esc voltar · o rato também funciona", "Cima/Baixo selecionar · Esquerda/Direita ou Enter alterar · Esc voltar · o mouse também funciona", "Arriba/Abajo elegir · Izquierda/Derecha o Enter cambiar · Esc volver · también con el ratón", "Haut/Bas choisir · Gauche/Droite ou Entrée modifier · Échap retour · la souris marche aussi", "Hoch/Runter wählen · Links/Rechts oder Enter ändern · Esc zurück · Maus geht auch", "Su/Giù scegli · Sinistra/Destra o Invio cambia · Esc indietro · funziona anche il mouse", "上下で選択 · 左右またはEnterで変更 · Escで戻る · マウスも使えます", "上/下 选择 · 左/右或回车 修改 · Esc 返回 · 也可以用鼠标", "위/아래 선택 · 좌/우 또는 Enter 변경 · Esc 뒤로 · 마우스도 사용 가능"],
"menu.resume": ["Resume", "Continuar", "Continuar", "Continuar", "Reprendre", "Fortsetzen", "Riprendi", "再開", "继续", "계속"],
"menu.save": ["Save game", "Guardar jogo", "Salvar jogo", "Guardar partida", "Sauvegarder", "Spiel speichern", "Salva partita", "セーブ", "保存游戏", "게임 저장"],
"menu.load": ["Load game", "Carregar jogo", "Carregar jogo", "Cargar partida", "Charger", "Spiel laden", "Carica partita", "ロード", "读取游戏", "게임 불러오기"],
"menu.quit": ["Quit game", "Sair do jogo", "Sair do jogo", "Salir del juego", "Quitter le jeu", "Spiel beenden", "Esci dal gioco", "ゲーム終了", "退出游戏", "게임 종료"],
"menu.gameplay": ["Gameplay", "Jogo", "Jogo", "Juego", "Jeu", "Spiel", "Gioco", "ゲームプレイ", "游戏", "게임플레이"],
"menu.peaceful": ["Peaceful mode (no creatures)", "Modo pacífico (sem criaturas)", "Modo pacífico (sem criaturas)", "Modo pacífico (sin criaturas)", "Mode paisible (sans créatures)", "Friedlicher Modus (keine Kreaturen)", "Modalità pacifica (niente creature)", "ピースフルモード（生物なし）", "和平模式（无生物）", "평화 모드 (생물 없음)"],
"menu.accessibility": ["Accessibility", "Acessibilidade", "Acessibilidade", "Accesibilidad", "Accessibilité", "Barrierefreiheit", "Accessibilità", "アクセシビリティ", "辅助功能", "접근성"],
"menu.language": ["Language", "Idioma", "Idioma", "Idioma", "Langue", "Sprache", "Lingua", "言語", "语言", "언어"],
"menu.uiscale": ["Interface scale", "Escala da interface", "Escala da interface", "Escala de la interfaz", "Taille de l'interface", "Oberflächengröße", "Scala dell'interfaccia", "UIの大きさ", "界面缩放", "인터페이스 크기"],
"menu.textsize": ["Text size (HUD boxes, code editor)", "Tamanho do texto (caixas HUD, editor)", "Tamanho do texto (caixas HUD, editor)", "Tamaño del texto (cajas HUD, editor)", "Taille du texte (cadres HUD, éditeur)", "Textgröße (HUD-Boxen, Editor)", "Dimensione testo (riquadri HUD, editor)", "文字の大きさ（HUD・エディタ）", "文字大小（HUD 框、编辑器）", "글자 크기 (HUD 상자, 편집기)"],
"menu.contrast": ["High contrast", "Alto contraste", "Alto contraste", "Alto contraste", "Contraste élevé", "Hoher Kontrast", "Alto contrasto", "ハイコントラスト", "高对比度", "고대비"],
"menu.colorblind": ["Colour-blind friendly palette", "Paleta para daltonismo", "Paleta para daltonismo", "Paleta para daltonismo", "Palette pour daltoniens", "Farbenblind-freundliche Palette", "Tavolozza per daltonici", "色覚多様性向けパレット", "色盲友好配色", "색각 이상 친화 팔레트"],
"menu.effects": ["Reduce flashes and particles", "Reduzir flashes e partículas", "Reduzir flashes e partículas", "Reducir destellos y partículas", "Réduire flashs et particules", "Blitze und Partikel reduzieren", "Riduci lampi e particelle", "フラッシュとパーティクルを減らす", "减少闪光和粒子", "섬광과 입자 줄이기"],
"menu.audio": ["Audio", "Áudio", "Áudio", "Audio", "Audio", "Audio", "Audio", "オーディオ", "音频", "오디오"],
"menu.vol.master": ["Master volume", "Volume geral", "Volume geral", "Volumen general", "Volume général", "Gesamtlautstärke", "Volume generale", "マスター音量", "主音量", "전체 음량"],
"menu.vol.music": ["Music", "Música", "Música", "Música", "Musique", "Musik", "Musica", "音楽", "音乐", "음악"],
"menu.vol.ambient": ["Ambience & weather", "Ambiente e clima", "Ambiente e clima", "Ambiente y clima", "Ambiance et météo", "Umgebung & Wetter", "Ambiente e meteo", "環境音・天候", "环境与天气", "환경음과 날씨"],
"menu.vol.machines": ["Machines", "Máquinas", "Máquinas", "Máquinas", "Machines", "Maschinen", "Macchine", "機械", "机器", "기계"],
"menu.vol.effects": ["Effects & alarms", "Efeitos e alarmes", "Efeitos e alarmes", "Efectos y alarmas", "Effets et alarmes", "Effekte & Alarme", "Effetti e allarmi", "効果音・アラーム", "音效与警报", "효과음과 경보"],
"menu.captions": ["Sound captions (visual indicators)", "Legendas de sons (indicadores visuais)", "Legendas de sons (indicadores visuais)", "Subtítulos de sonidos (indicadores visuales)", "Sous-titres des sons (indicateurs visuels)", "Geräusch-Untertitel (visuelle Hinweise)", "Didascalie dei suoni (indicatori visivi)", "効果音字幕（視覚表示）", "声音字幕（视觉提示）", "소리 자막 (시각 표시)"],
"menu.controls": ["Controls (Enter, then press a key)", "Teclas (Enter e depois a tecla)", "Controles (Enter e depois a tecla)", "Controles (Enter y luego la tecla)", "Commandes (Entrée puis la touche)", "Steuerung (Enter, dann Taste drücken)", "Comandi (Invio, poi il tasto)", "操作（Enterの後にキーを押す）", "按键（回车后按下按键）", "조작 (Enter 후 키 입력)"],
"menu.resetcontrols": ["Reset controls to defaults", "Repor teclas predefinidas", "Restaurar controles padrão", "Restablecer controles", "Réinitialiser les commandes", "Steuerung zurücksetzen", "Ripristina comandi", "操作を初期設定に戻す", "恢复默认按键", "기본 조작으로 초기화"],
"menu.presskey": ["press a key…", "prime uma tecla…", "pressione uma tecla…", "pulsa una tecla…", "appuyez sur une touche…", "Taste drücken…", "premi un tasto…", "キーを押してください…", "请按一个键…", "키를 누르세요…"],
"menu.on": ["ON", "SIM", "SIM", "SÍ", "OUI", "AN", "SÌ", "オン", "开", "켬"],
"menu.off": ["off", "não", "não", "no", "non", "aus", "no", "オフ", "关", "끔"],
"control.up": ["Move up", "Mover para cima", "Mover para cima", "Mover arriba", "Haut", "Nach oben", "Su", "上へ移動", "向上移动", "위로 이동"],
"control.down": ["Move down", "Mover para baixo", "Mover para baixo", "Mover abajo", "Bas", "Nach unten", "Giù", "下へ移動", "向下移动", "아래로 이동"],
"control.left": ["Move left", "Mover para a esquerda", "Mover para a esquerda", "Mover a la izquierda", "Gauche", "Nach links", "Sinistra", "左へ移動", "向左移动", "왼쪽으로 이동"],
"control.right": ["Move right", "Mover para a direita", "Mover para a direita", "Mover a la derecha", "Droite", "Nach rechts", "Destra", "右へ移動", "向右移动", "오른쪽으로 이동"],
"control.build": ["Build mode", "Modo construção", "Modo construção", "Modo construcción", "Mode construction", "Baumodus", "Modalità costruzione", "建設モード", "建造模式", "건설 모드"],
"control.rotate": ["Rotate", "Rodar", "Girar", "Girar", "Pivoter", "Drehen", "Ruota", "回転", "旋转", "회전"],
"control.interact": ["Open machine / computer", "Abrir máquina / computador", "Abrir máquina / computador", "Abrir máquina / ordenador", "Ouvrir machine / ordinateur", "Maschine / Computer öffnen", "Apri macchina / computer", "機械・コンピュータを開く", "打开机器 / 计算机", "기계 / 컴퓨터 열기"],
"control.eat": ["Eat", "Comer", "Comer", "Comer", "Manger", "Essen", "Mangia", "食べる", "进食", "먹기"],
"control.research": ["Research tree", "Árvore de investigação", "Árvore de pesquisa", "Árbol de investigación", "Arbre de recherche", "Forschungsbaum", "Albero della ricerca", "研究ツリー", "研究树", "연구 트리"],
"control.inventory": ["Inventory", "Inventário", "Inventário", "Inventario", "Inventaire", "Inventar", "Inventario", "インベントリ", "背包", "인벤토리"],
"control.save": ["Quick save", "Guardar rápido", "Salvar rápido", "Guardado rápido", "Sauvegarde rapide", "Schnellspeichern", "Salvataggio rapido", "クイックセーブ", "快速保存", "빠른 저장"],
"control.load": ["Quick load", "Carregar rápido", "Carregar rápido", "Carga rápida", "Chargement rapide", "Schnellladen", "Caricamento rapido", "クイックロード", "快速读取", "빠른 불러오기"],
"control.journal": ["Journal", "Diário", "Diário", "Diario", "Journal", "Tagebuch", "Diario", "日誌", "日志", "일지"],
"notify.log": ["Log recovered: {0}", "Registo recuperado: {0}", "Registro recuperado: {0}", "Registro recuperado: {0}", "Journal récupéré : {0}", "Protokoll geborgen: {0}", "Registro recuperato: {0}", "ログを回収: {0}", "已恢复日志：{0}", "기록 복구: {0}"],
"notify.loot": ["Supply cache opened at {0}", "Caixa de mantimentos aberta em {0}", "Caixa de suprimentos aberta em {0}", "Alijo de suministros abierto en {0}", "Cache de ravitaillement ouverte à {0}", "Vorratskiste geöffnet bei {0}", "Cassa di rifornimenti aperta a {0}", "{0}の補給箱を開けた", "已打开 {0} 的补给箱", "{0}의 보급 상자를 열었습니다"],
"explore.empty": ["This cache is empty", "Esta caixa está vazia", "Esta caixa está vazia", "Este alijo está vacío", "Cette cache est vide", "Diese Kiste ist leer", "Questa cassa è vuota", "この箱は空です", "这个箱子是空的", "이 상자는 비어 있습니다"],
"explore.full": ["Inventory full", "Inventário cheio", "Inventário cheio", "Inventario lleno", "Inventaire plein", "Inventar voll", "Inventario pieno", "インベントリが満杯", "背包已满", "인벤토리가 가득 찼습니다"],
"explore.journal": ["Journal — recovered logs ({0}/{1})", "Diário — registos recuperados ({0}/{1})", "Diário — registros recuperados ({0}/{1})", "Diario — registros recuperados ({0}/{1})", "Journal — rapports récupérés ({0}/{1})", "Tagebuch — geborgene Protokolle ({0}/{1})", "Diario — registri recuperati ({0}/{1})", "日誌 — 回収したログ ({0}/{1})", "日志 — 已恢复的记录（{0}/{1}）", "일지 — 복구한 기록 ({0}/{1})"],
"explore.nologs": ["No logs yet. Survey outposts stand in the Ruins biome — read their terminals (E).", "Ainda sem registos. Os postos de reconhecimento ficam no bioma Ruínas — lê os terminais (E).", "Ainda sem registros. Os postos de reconhecimento ficam no bioma Ruínas — leia os terminais (E).", "Aún no hay registros. Los puestos de exploración están en el bioma Ruinas — lee sus terminales (E).", "Aucun rapport. Les avant-postes se trouvent dans le biome Ruines — lisez leurs terminaux (E).", "Noch keine Protokolle. Außenposten stehen im Ruinen-Biom — lies ihre Terminals (E).", "Nessun registro. Gli avamposti sono nel bioma Rovine — leggi i loro terminali (E).", "ログはまだありません。調査拠点は遺跡バイオームにあります — 端末を読もう (E)。", "还没有记录。勘测前哨位于废墟生物群系 — 读取终端（E）。", "아직 기록이 없습니다. 조사 기지는 폐허 지역에 있습니다 — 단말기를 읽으세요 (E)."],
"explore.signal": ["Scanner: unread outpost {0} cells {1}", "Scanner: posto por ler a {0} células {1}", "Scanner: posto não lido a {0} células {1}", "Escáner: puesto sin leer a {0} casillas {1}", "Scanner : avant-poste non lu à {0} cases {1}", "Scanner: ungelesener Außenposten {0} Felder {1}", "Scanner: avamposto non letto a {0} celle {1}", "スキャナー: 未読の拠点 {0}マス {1}", "扫描仪：未读前哨 {0} 格 {1}", "스캐너: 미확인 기지 {0}칸 {1}"],
"explore.nosignal": ["Scanner: no unread outpost within {0} cells", "Scanner: nenhum posto por ler a menos de {0} células", "Scanner: nenhum posto não lido a menos de {0} células", "Escáner: ningún puesto sin leer a menos de {0} casillas", "Scanner : aucun avant-poste non lu à moins de {0} cases", "Scanner: kein ungelesener Außenposten innerhalb {0} Felder", "Scanner: nessun avamposto non letto entro {0} celle", "スキャナー: {0}マス以内に未読の拠点なし", "扫描仪：{0} 格内没有未读前哨", "스캐너: {0}칸 안에 미확인 기지 없음"],
"explore.close": ["({0} / Esc to close · {1} journal)", "({0} / Esc para fechar · {1} diário)", "({0} / Esc para fechar · {1} diário)", "({0} / Esc para cerrar · {1} diario)", "({0} / Échap pour fermer · {1} journal)", "({0} / Esc zum Schließen · {1} Tagebuch)", "({0} / Esc per chiudere · {1} diario)", "（{0} / Esc で閉じる · {1} 日誌）", "（{0} / Esc 关闭 · {1} 日志）", "({0} / Esc로 닫기 · {1} 일지)"],
"control.network": ["Multiplayer", "Multijogador", "Multijogador", "Multijugador", "Multijoueur", "Mehrspieler", "Multigiocatore", "マルチプレイ", "多人游戏", "멀티플레이"],
"notify.joined": ["{0} joined the game", "{0} entrou no jogo", "{0} entrou no jogo", "{0} se unió a la partida", "{0} a rejoint la partie", "{0} ist dem Spiel beigetreten", "{0} è entrato in partita", "{0}が参加しました", "{0} 加入了游戏", "{0}님이 참가했습니다"],
"notify.left": ["{0} left the game", "{0} saiu do jogo", "{0} saiu do jogo", "{0} salió de la partida", "{0} a quitté la partie", "{0} hat das Spiel verlassen", "{0} ha lasciato la partita", "{0}が退出しました", "{0} 离开了游戏", "{0}님이 나갔습니다"],
"notify.disconnected": ["Multiplayer: {0}", "Multijogador: {0}", "Multijogador: {0}", "Multijugador: {0}", "Multijoueur : {0}", "Mehrspieler: {0}", "Multigiocatore: {0}", "マルチプレイ: {0}", "多人游戏：{0}", "멀티플레이: {0}"],
"net.title": ["LAN multiplayer", "Multijogador em rede local", "Multijogador em rede local", "Multijugador en red local", "Multijoueur en réseau local", "LAN-Mehrspieler", "Multigiocatore in LAN", "LANマルチプレイ", "局域网多人游戏", "LAN 멀티플레이"],
"net.name": ["Name", "Nome", "Nome", "Nombre", "Nom", "Name", "Nome", "名前", "名字", "이름"],
"net.address": ["Host IP", "IP do anfitrião", "IP do anfitrião", "IP del anfitrión", "IP de l'hôte", "Host-IP", "IP dell'host", "ホストIP", "主机 IP", "호스트 IP"],
"net.host": ["Host this world", "Abrir este mundo", "Hospedar este mundo", "Alojar este mundo", "Héberger ce monde", "Diese Welt hosten", "Ospita questo mondo", "この世界をホスト", "主持这个世界", "이 세계 호스트"],
"net.join": ["Join", "Entrar", "Entrar", "Unirse", "Rejoindre", "Beitreten", "Entra", "参加", "加入", "참가"],
"net.help": ["Same network only (port {0}). Guests play in the host's world with their own inventory; programs run on the host's computer, in its sandbox.", "Só na mesma rede (porta {0}). Os convidados jogam no mundo do anfitrião com inventário próprio; os programas correm no computador do anfitrião, no seu isolamento.", "Só na mesma rede (porta {0}). Os convidados jogam no mundo do anfitrião com inventário próprio; os programas rodam no computador do anfitrião, isolados.", "Solo en la misma red (puerto {0}). Los invitados juegan en el mundo del anfitrión con su propio inventario; los programas se ejecutan en el ordenador del anfitrión, aislados.", "Même réseau uniquement (port {0}). Les invités jouent dans le monde de l'hôte avec leur propre inventaire ; les programmes tournent sur l'ordinateur de l'hôte, isolés.", "Nur im selben Netzwerk (Port {0}). Gäste spielen in der Welt des Hosts mit eigenem Inventar; Programme laufen isoliert auf dem Rechner des Hosts.", "Solo nella stessa rete (porta {0}). Gli ospiti giocano nel mondo dell'host con il proprio inventario; i programmi girano sul computer dell'host, isolati.", "同じネットワークのみ（ポート {0}）。ゲストは自分のインベントリでホストの世界を遊びます。プログラムはホストのPCで隔離して実行されます。", "仅限同一网络（端口 {0}）。访客使用自己的背包在主机的世界中游戏；程序在主机电脑的沙箱中运行。", "같은 네트워크에서만 (포트 {0}). 손님은 자신의 인벤토리로 호스트의 세계에서 플레이하며, 프로그램은 호스트 컴퓨터의 샌드박스에서 실행됩니다."],
"net.makevisitor": ["Make visitor", "Tornar visitante", "Tornar visitante", "Hacer visitante", "Rendre visiteur", "Zum Besucher", "Rendi visitatore", "見学者にする", "设为访客", "방문자로"],
"net.makebuilder": ["Make builder", "Tornar construtor", "Tornar construtor", "Hacer constructor", "Rendre bâtisseur", "Zum Baumeister", "Rendi costruttore", "建築者にする", "设为建造者", "건설자로"],
"net.kick": ["Kick", "Expulsar", "Expulsar", "Expulsar", "Exclure", "Entfernen", "Espelli", "キック", "踢出", "강퇴"],
"net.role": ["Your role: {0}", "O teu papel: {0}", "Seu papel: {0}", "Tu rol: {0}", "Votre rôle : {0}", "Deine Rolle: {0}", "Il tuo ruolo: {0}", "あなたの役割: {0}", "你的角色：{0}", "내 역할: {0}"],
"net.disconnect": ["Disconnect", "Desligar", "Desconectar", "Desconectar", "Se déconnecter", "Trennen", "Disconnetti", "切断", "断开连接", "연결 끊기"],
"net.close": ["Close (Esc)", "Fechar (Esc)", "Fechar (Esc)", "Cerrar (Esc)", "Fermer (Échap)", "Schließen (Esc)", "Chiudi (Esc)", "閉じる (Esc)", "关闭（Esc）", "닫기 (Esc)"],
"building.launchpad": ["Launch Pad", "Plataforma de lançamento", "Plataforma de lançamento", "Plataforma de lanzamiento", "Pas de tir", "Startrampe", "Rampa di lancio", "発射台", "发射台", "발사대"],
"notify.rebuilt": ["The ship is rebuilt! Build a Launch Pad near a computer, fuel it with coal or crude oil, and fly the ascent with a program.", "A nave está reconstruída! Constrói uma Plataforma de lançamento perto de um computador, abastece-a com carvão ou petróleo e pilota a subida com um programa.", "A nave está reconstruída! Construa uma Plataforma de lançamento perto de um computador, abasteça com carvão ou petróleo e pilote a subida com um programa.", "¡La nave está reconstruida! Construye una Plataforma de lanzamiento cerca de un ordenador, cárgala con carbón o petróleo y pilota el ascenso con un programa.", "Le vaisseau est reconstruit ! Construisez un pas de tir près d'un ordinateur, remplissez-le de charbon ou de pétrole et pilotez l'ascension avec un programme.", "Das Schiff ist wiederaufgebaut! Baue eine Startrampe nahe einem Computer, betanke sie mit Kohle oder Rohöl und fliege den Aufstieg mit einem Programm.", "La nave è ricostruita! Costruisci una rampa di lancio vicino a un computer, riforniscila di carbone o petrolio e pilota la salita con un programma.", "宇宙船が再建された！コンピュータの近くに発射台を建て、石炭か原油を積み、プログラムで上昇を操縦しよう。", "飞船已重建！在计算机附近建造发射台，装入煤或原油，用程序驾驶上升。", "우주선이 재건되었습니다! 컴퓨터 근처에 발사대를 짓고 석탄이나 원유를 채운 뒤 프로그램으로 상승을 조종하세요."],
"notify.orbit": ["ORBIT REACHED above {0}! Programs can now list worlds with pad.planets() and fly with pad.travel(id).", "ÓRBITA ATINGIDA sobre {0}! Os programas podem listar mundos com pad.planets() e voar com pad.travel(id).", "ÓRBITA ALCANÇADA sobre {0}! Os programas podem listar mundos com pad.planets() e voar com pad.travel(id).", "¡ÓRBITA ALCANZADA sobre {0}! Los programas pueden listar mundos con pad.planets() y volar con pad.travel(id).", "ORBITE ATTEINTE au-dessus de {0} ! Les programmes peuvent lister les mondes avec pad.planets() et voler avec pad.travel(id).", "ORBIT ERREICHT über {0}! Programme können Welten mit pad.planets() auflisten und mit pad.travel(id) fliegen.", "ORBITA RAGGIUNTA sopra {0}! I programmi possono elencare i mondi con pad.planets() e volare con pad.travel(id).", "{0}の軌道に到達！プログラムで pad.planets() で世界を一覧し、pad.travel(id) で飛べます。", "已进入 {0} 上空轨道！程序可用 pad.planets() 列出世界，用 pad.travel(id) 飞行。", "{0} 궤도 도달! 프로그램에서 pad.planets()로 세계를 보고 pad.travel(id)로 비행할 수 있습니다."],
"notify.aborted": ["Launch {0}. The capsule parachuted back to the pad.", "Lançamento {0}. A cápsula voltou de paraquedas à plataforma.", "Lançamento {0}. A cápsula voltou de paraquedas à plataforma.", "Lanzamiento {0}. La cápsula volvió en paracaídas a la plataforma.", "Lancement {0}. La capsule est redescendue en parachute sur le pas de tir.", "Start {0}. Die Kapsel schwebte am Fallschirm zur Rampe zurück.", "Lancio {0}. La capsula è tornata col paracadute sulla rampa.", "打ち上げ {0}。カプセルはパラシュートで発射台に戻りました。", "发射 {0}。返回舱乘降落伞回到发射台。", "발사 {0}. 캡슐이 낙하산으로 발사대에 돌아왔습니다."],
"notify.landed": ["Landed on {0}. Your base on the previous world was saved and waits for you.", "Aterraste em {0}. A tua base no mundo anterior ficou guardada à tua espera.", "Você pousou em {0}. Sua base no mundo anterior foi salva e espera por você.", "Aterrizaste en {0}. Tu base del mundo anterior quedó guardada y te espera.", "Atterrissage sur {0}. Votre base sur le monde précédent a été sauvegardée et vous attend.", "Auf {0} gelandet. Deine Basis auf der vorigen Welt wurde gespeichert und wartet auf dich.", "Atterrato su {0}. La tua base sul mondo precedente è salvata e ti aspetta.", "{0}に着陸。前の世界の基地は保存され、あなたを待っています。", "已降落在 {0}。你在上一个世界的基地已保存，等你回来。", "{0}에 착륙했습니다. 이전 세계의 기지는 저장되어 기다리고 있습니다."],
"title.continue": ["Continue", "Continuar", "Continuar", "Continuar", "Continuer", "Fortsetzen", "Continua", "続きから", "继续游戏", "이어하기"],
"title.new": ["New game", "Novo jogo", "Novo jogo", "Nueva partida", "Nouvelle partie", "Neues Spiel", "Nuova partita", "ニューゲーム", "新游戏", "새 게임"],
"title.tagline": ["Build. Code. Survive. Explore.", "Constrói. Programa. Sobrevive. Explora.", "Construa. Programe. Sobreviva. Explore.", "Construye. Programa. Sobrevive. Explora.", "Construis. Programme. Survis. Explore.", "Bauen. Programmieren. Überleben. Erkunden.", "Costruisci. Programma. Sopravvivi. Esplora.", "建てる。書く。生き延びる。探検する。", "建造。编程。生存。探索。", "건설. 코딩. 생존. 탐험."],
"title.seed": ["Seed (optional)", "Seed (opcional)", "Seed (opcional)", "Semilla (opcional)", "Graine (facultatif)", "Seed (optional)", "Seme (facoltativo)", "シード（任意）", "种子（可选）", "시드 (선택)"],
"title.keys": ["Up/Down and Enter, or the mouse · options later with Esc", "Cima/Baixo e Enter, ou o rato · opções depois com Esc", "Cima/Baixo e Enter, ou o mouse · opções depois com Esc", "Arriba/Abajo y Enter, o el ratón · opciones luego con Esc", "Haut/Bas et Entrée, ou la souris · options ensuite avec Échap", "Hoch/Runter und Enter oder Maus · Optionen später mit Esc", "Su/Giù e Invio, o il mouse · opzioni poi con Esc", "上下とEnter、またはマウス · オプションは後でEsc", "上/下和回车，或鼠标 · 之后按 Esc 打开选项", "위/아래와 Enter 또는 마우스 · 옵션은 나중에 Esc"],
"hud.inventory": ["Inventory", "Inventário", "Inventário", "Inventario", "Inventaire", "Inventar", "Inventario", "インベントリ", "背包", "인벤토리"],
"hud.inventory.empty": ["empty — hold LMB on trees/ore", "vazio — mantém o botão esquerdo em árvores/minério", "vazio — segure o botão esquerdo em árvores/minério", "vacío — mantén el clic izquierdo en árboles/mineral", "vide — maintenez le clic gauche sur arbres/minerai", "leer — Linksklick auf Bäumen/Erz halten", "vuoto — tieni premuto il tasto sinistro su alberi/minerale", "空 — 木や鉱石の上で左クリック長押し", "空 — 在树木/矿石上按住左键", "비어 있음 — 나무/광석에서 왼쪽 버튼을 누르세요"],
"hud.help": ["{0} build · hold LMB mine/drink · {1} eat · {2} open machine · {3} research · move {4} · wheel zoom · Esc options", "{0} construir · LMB minerar/beber · {1} comer · {2} abrir máquina · {3} investigação · mover {4} · roda zoom · Esc opções", "{0} construir · LMB minerar/beber · {1} comer · {2} abrir máquina · {3} pesquisa · mover {4} · roda zoom · Esc opções", "{0} construir · clic izq. minar/beber · {1} comer · {2} abrir máquina · {3} investigación · mover {4} · rueda zoom · Esc opciones", "{0} construire · clic gauche miner/boire · {1} manger · {2} ouvrir machine · {3} recherche · déplacer {4} · molette zoom · Échap options", "{0} bauen · Linksklick abbauen/trinken · {1} essen · {2} Maschine öffnen · {3} Forschung · bewegen {4} · Mausrad Zoom · Esc Optionen", "{0} costruisci · tasto sinistro scava/bevi · {1} mangia · {2} apri macchina · {3} ricerca · muovi {4} · rotella zoom · Esc opzioni", "{0} 建設 · 左長押しで採掘/飲む · {1} 食べる · {2} 機械を開く · {3} 研究 · 移動 {4} · ホイールでズーム · Esc オプション", "{0} 建造 · 按住左键 采矿/喝水 · {1} 进食 · {2} 打开机器 · {3} 研究 · 移动 {4} · 滚轮缩放 · Esc 选项", "{0} 건설 · 왼쪽 버튼 채굴/마시기 · {1} 먹기 · {2} 기계 열기 · {3} 연구 · 이동 {4} · 휠 확대 · Esc 옵션"],
"hud.buildmode": ["BUILD MODE — R rotate ({0}) · LMB place · RMB remove · Esc exit", "MODO CONSTRUÇÃO — R rodar ({0}) · LMB colocar · RMB remover · Esc sair", "MODO CONSTRUÇÃO — R girar ({0}) · LMB colocar · RMB remover · Esc sair", "MODO CONSTRUCCIÓN — R girar ({0}) · clic izq. colocar · clic der. quitar · Esc salir", "MODE CONSTRUCTION — R pivoter ({0}) · clic gauche placer · clic droit retirer · Échap quitter", "BAUMODUS — R drehen ({0}) · Linksklick setzen · Rechtsklick entfernen · Esc beenden", "MODALITÀ COSTRUZIONE — R ruota ({0}) · sinistro piazza · destro rimuovi · Esc esci", "建設モード — R 回転 ({0}) · 左クリック 設置 · 右クリック 撤去 · Esc 終了", "建造模式 — R 旋转 ({0}) · 左键 放置 · 右键 拆除 · Esc 退出", "건설 모드 — R 회전 ({0}) · 왼쪽 버튼 설치 · 오른쪽 버튼 철거 · Esc 종료"],
"hud.needs": ["research: {0}", "investigar: {0}", "pesquisar: {0}", "investigar: {0}", "recherche : {0}", "Forschung: {0}", "ricerca: {0}", "研究: {0}", "需研究：{0}", "연구 필요: {0}"],
"research.title": ["Research", "Investigação", "Pesquisa", "Investigación", "Recherche", "Forschung", "Ricerca", "研究", "研究", "연구"],
"research.ship": ["Ship rebuilt: {0}/{1}", "Nave reconstruída: {0}/{1}", "Nave reconstruída: {0}/{1}", "Nave reconstruida: {0}/{1}", "Vaisseau reconstruit : {0}/{1}", "Schiff wiederaufgebaut: {0}/{1}", "Nave ricostruita: {0}/{1}", "宇宙船の再建: {0}/{1}", "飞船重建：{0}/{1}", "우주선 재건: {0}/{1}"],
"research.close": ["(T / Esc to close)", "(T / Esc para fechar)", "(T / Esc para fechar)", "(T / Esc para cerrar)", "(T / Échap pour fermer)", "(T / Esc zum Schließen)", "(T / Esc per chiudere)", "（T / Esc で閉じる）", "（T / Esc 关闭）", "(T / Esc로 닫기)"],
"research.how": ["Research runs in a Research Lab commanded by a program. Put the cost items in the lab (E), then:", "A investigação corre num Laboratório comandado por um programa. Põe os materiais no laboratório (E) e depois:", "A pesquisa roda num Laboratório comandado por um programa. Coloque os materiais no laboratório (E) e depois:", "La investigación se hace en un Laboratorio dirigido por un programa. Pon los materiales en el laboratorio (E) y luego:", "La recherche se fait dans un Laboratoire piloté par un programme. Placez les matériaux dans le laboratoire (E), puis :", "Geforscht wird in einem Labor, das ein Programm steuert. Lege die Kosten ins Labor (E), dann:", "La ricerca avviene in un Laboratorio comandato da un programma. Metti i materiali nel laboratorio (E), poi:", "研究はプログラムで動かす研究所で行います。材料を研究所に入れて（E）、次に:", "研究在由程序控制的研究实验室中进行。把所需材料放入实验室（E），然后：", "연구는 프로그램이 제어하는 연구소에서 진행됩니다. 재료를 연구소에 넣고(E), 그다음:"],
"research.mainline": ["Main line — rebuild the ship", "Linha principal — reconstruir a nave", "Linha principal — reconstruir a nave", "Línea principal — reconstruir la nave", "Ligne principale — reconstruire le vaisseau", "Hauptlinie — das Schiff wiederaufbauen", "Linea principale — ricostruire la nave", "メインライン — 宇宙船を再建する", "主线 — 重建飞船", "메인 라인 — 우주선 재건"],
"area.Industry": ["Industry", "Indústria", "Indústria", "Industria", "Industrie", "Industrie", "Industria", "産業", "工业", "산업"],
"area.Science": ["Science", "Ciência", "Ciência", "Ciencia", "Science", "Wissenschaft", "Scienza", "科学", "科学", "과학"],
"area.Computing": ["Computing", "Computação", "Computação", "Computación", "Informatique", "Datenverarbeitung", "Informatica", "コンピューティング", "计算", "컴퓨팅"],
"area.Energy": ["Energy", "Energia", "Energia", "Energía", "Énergie", "Energie", "Energia", "エネルギー", "能源", "에너지"],
"area.Security": ["Security", "Segurança", "Segurança", "Seguridad", "Sécurité", "Sicherheit", "Sicurezza", "セキュリティ", "安全", "보안"],
"area.Defense": ["Defense", "Defesa", "Defesa", "Defensa", "Défense", "Verteidigung", "Difesa", "防衛", "防御", "방어"],
"area.Space": ["Space", "Espaço", "Espaço", "Espacio", "Espace", "Raumfahrt", "Spazio", "宇宙", "太空", "우주"],
"research.cost": ["Cost", "Custo", "Custo", "Coste", "Coût", "Kosten", "Costo", "コスト", "成本", "비용"],
"research.needs": ["Needs", "Requer", "Requer", "Requiere", "Requiert", "Benötigt", "Richiede", "必要", "需要", "필요"],
"research.unlocks": ["Unlocks", "Desbloqueia", "Desbloqueia", "Desbloquea", "Débloque", "Schaltet frei", "Sblocca", "解放", "解锁", "해금"],
"research.done": ["done", "feito", "concluído", "hecho", "terminé", "fertig", "fatto", "完了", "已完成", "완료"],
"research.available": ["available", "disponível", "disponível", "disponible", "disponible", "verfügbar", "disponibile", "研究可能", "可研究", "가능"],
"research.locked": ["locked", "bloqueado", "bloqueado", "bloqueado", "verrouillé", "gesperrt", "bloccato", "未解放", "未解锁", "잠김"],
"notify.wave": ["{0} hostile creature(s) approaching — your industry is noisy!", "{0} criatura(s) hostil(is) a aproximar-se — a tua indústria faz barulho!", "{0} criatura(s) hostil(is) se aproximando — sua indústria faz barulho!", "¡{0} criatura(s) hostil(es) se acerca(n) — tu industria hace ruido!", "{0} créature(s) hostile(s) approche(nt) — votre industrie est bruyante !", "{0} feindliche Kreatur(en) nähern sich — deine Industrie ist laut!", "{0} creatura/e ostile/i in arrivo — la tua industria è rumorosa!", "{0}体の敵性生物が接近中 — 工場の騒音が原因です！", "{0} 个敌对生物正在接近 — 你的工业太吵了！", "적대 생물 {0}마리 접근 중 — 산업 소음 때문입니다!"],
"notify.destroyed": ["{0} was destroyed", "{0} foi destruído", "{0} foi destruído", "{0} fue destruido", "{0} a été détruit", "{0} wurde zerstört", "{0} è stato distrutto", "{0}が破壊されました", "{0} 被摧毁了", "{0}이(가) 파괴되었습니다"],
"notify.compromised": ["SECURITY: PC-{0} was compromised by a rogue drone", "SEGURANÇA: o PC-{0} foi comprometido por um drone hostil", "SEGURANÇA: o PC-{0} foi comprometido por um drone hostil", "SEGURIDAD: el PC-{0} fue comprometido por un dron hostil", "SÉCURITÉ : le PC-{0} a été compromis par un drone hostile", "SICHERHEIT: PC-{0} wurde von einer feindlichen Drohne kompromittiert", "SICUREZZA: il PC-{0} è stato compromesso da un drone ostile", "セキュリティ: PC-{0}が敵性ドローンに侵害されました", "安全：PC-{0} 被敌对无人机入侵", "보안: PC-{0}이(가) 적대 드론에 감염되었습니다"],
"notify.intrusion": ["INTRUSION: a drone is breaking into PC-{0} ({1} s)", "INTRUSÃO: um drone está a invadir o PC-{0} ({1} s)", "INTRUSÃO: um drone está invadindo o PC-{0} ({1} s)", "INTRUSIÓN: un dron está entrando en el PC-{0} ({1} s)", "INTRUSION : un drone pénètre dans le PC-{0} ({1} s)", "EINDRINGLING: Eine Drohne bricht in PC-{0} ein ({1} s)", "INTRUSIONE: un drone sta violando il PC-{0} ({1} s)", "侵入: ドローンがPC-{0}に侵入中（{1}秒）", "入侵：无人机正在入侵 PC-{0}（{1} 秒）", "침입: 드론이 PC-{0}에 침입 중 ({1}초)"],
"notify.research": ["Research complete: {0}", "Investigação concluída: {0}", "Pesquisa concluída: {0}", "Investigación completada: {0}", "Recherche terminée : {0}", "Forschung abgeschlossen: {0}", "Ricerca completata: {0}", "研究完了: {0}", "研究完成：{0}", "연구 완료: {0}"],
"notify.ship": ["Ship progress {0}/{1}: {2} complete", "Progresso da nave {0}/{1}: {2} concluído", "Progresso da nave {0}/{1}: {2} concluído", "Progreso de la nave {0}/{1}: {2} completado", "Progrès du vaisseau {0}/{1} : {2} terminé", "Schiffsfortschritt {0}/{1}: {2} fertig", "Progresso nave {0}/{1}: {2} completato", "宇宙船の進捗 {0}/{1}: {2} 完了", "飞船进度 {0}/{1}：{2} 完成", "우주선 진행 {0}/{1}: {2} 완료"],
"notify.collapsed": ["You collapsed — back at the crash site", "Desmaiaste — de volta ao local do acidente", "Você desmaiou — de volta ao local do acidente", "Te desmayaste — de vuelta al lugar del accidente", "Vous vous êtes effondré — retour au site du crash", "Du bist zusammengebrochen — zurück an der Absturzstelle", "Sei svenuto — di nuovo al luogo dello schianto", "倒れました — 墜落地点に戻ります", "你倒下了 — 回到坠毁地点", "쓰러졌습니다 — 추락 지점으로 돌아갑니다"],
"building.wall": ["Stone Wall", "Muro de pedra", "Muro de pedra", "Muro de piedra", "Mur de pierre", "Steinmauer", "Muro di pietra", "石の壁", "石墙", "돌벽"],
"building.chest": ["Storage Chest", "Baú", "Baú", "Cofre", "Coffre", "Lagerkiste", "Baule", "収納箱", "储物箱", "보관함"],
"building.furnace": ["Stone Furnace", "Fornalha de pedra", "Fornalha de pedra", "Horno de piedra", "Four en pierre", "Steinofen", "Fornace di pietra", "石の炉", "石炉", "돌 용광로"],
"building.drill": ["Electric Drill", "Perfuradora elétrica", "Perfuratriz elétrica", "Taladro eléctrico", "Foreuse électrique", "Elektrischer Bohrer", "Trivella elettrica", "電動ドリル", "电钻", "전동 드릴"],
"building.computer": ["Basic Computer", "Computador básico", "Computador básico", "Ordenador básico", "Ordinateur de base", "Einfacher Computer", "Computer base", "基本コンピュータ", "基础计算机", "기본 컴퓨터"],
"building.generator": ["Coal Generator", "Gerador a carvão", "Gerador a carvão", "Generador de carbón", "Générateur à charbon", "Kohlegenerator", "Generatore a carbone", "石炭発電機", "燃煤发电机", "석탄 발전기"],
"building.pole": ["Power Pole", "Poste elétrico", "Poste elétrico", "Poste eléctrico", "Poteau électrique", "Strommast", "Palo elettrico", "電柱", "电线杆", "전신주"],
"building.lab": ["Research Lab", "Laboratório", "Laboratório", "Laboratorio", "Laboratoire", "Forschungslabor", "Laboratorio", "研究所", "研究实验室", "연구소"],
"building.belt": ["Transport Belt", "Correia transportadora", "Esteira", "Cinta transportadora", "Tapis roulant", "Förderband", "Nastro trasportatore", "ベルトコンベア", "传送带", "운송 벨트"],
"building.conveyor": ["Programmable Conveyor", "Transportador programável", "Transportador programável", "Transportador programable", "Convoyeur programmable", "Programmierbarer Förderer", "Trasportatore programmabile", "プログラム式コンベア", "可编程传送机", "프로그래밍 컨베이어"],
"building.inserter": ["Inserter", "Braço", "Braço", "Insertador", "Bras robotisé", "Greifarm", "Braccio", "インサーター", "机械臂", "투입기"],
"building.greenhouse": ["Greenhouse", "Estufa", "Estufa", "Invernadero", "Serre", "Gewächshaus", "Serra", "温室", "温室", "온실"],
"building.assembler": ["Assembler", "Montadora", "Montadora", "Ensambladora", "Assembleur", "Montagemaschine", "Assemblatore", "組立機", "组装机", "조립기"],
"building.turret": ["Defense Turret", "Torre de defesa", "Torre de defesa", "Torreta defensiva", "Tourelle", "Verteidigungsturm", "Torretta difensiva", "防衛タレット", "防御炮塔", "방어 포탑"],
"building.solar": ["Solar Panel", "Painel solar", "Painel solar", "Panel solar", "Panneau solaire", "Solarmodul", "Pannello solare", "ソーラーパネル", "太阳能板", "태양광 패널"],
"building.battery": ["Battery", "Bateria", "Bateria", "Batería", "Batterie", "Batterie", "Batteria", "バッテリー", "电池", "배터리"],
"building.computer2": ["Advanced Computer", "Computador avançado", "Computador avançado", "Ordenador avanzado", "Ordinateur avancé", "Fortgeschrittener Computer", "Computer avanzato", "高性能コンピュータ", "高级计算机", "고급 컴퓨터"],
"building.scrubber": ["Air Scrubber", "Purificador de ar", "Purificador de ar", "Depurador de aire", "Épurateur d'air", "Luftwäscher", "Depuratore d'aria", "空気清浄機", "空气净化器", "공기 정화기"],
"tech.ecology": ["Ecology", "Ecologia", "Ecologia", "Ecología", "Écologie", "Ökologie", "Ecologia", "生態学", "生态", "생태학"],
"item.wood": ["Wood", "Madeira", "Madeira", "Madera", "Bois", "Holz", "Legno", "木材", "木材", "나무"],
"item.stone": ["Stone", "Pedra", "Pedra", "Piedra", "Pierre", "Stein", "Pietra", "石", "石头", "돌"],
"item.iron_ore": ["Iron Ore", "Minério de ferro", "Minério de ferro", "Mineral de hierro", "Minerai de fer", "Eisenerz", "Minerale di ferro", "鉄鉱石", "铁矿石", "철광석"],
"item.copper_ore": ["Copper Ore", "Minério de cobre", "Minério de cobre", "Mineral de cobre", "Minerai de cuivre", "Kupfererz", "Minerale di rame", "銅鉱石", "铜矿石", "구리 광석"],
"item.coal": ["Coal", "Carvão", "Carvão", "Carbón", "Charbon", "Kohle", "Carbone", "石炭", "煤", "석탄"],
"item.silicon": ["Silicon", "Silício", "Silício", "Silicio", "Silicium", "Silizium", "Silicio", "シリコン", "硅", "실리콘"],
"item.titanium_ore": ["Titanium Ore", "Minério de titânio", "Minério de titânio", "Mineral de titanio", "Minerai de titane", "Titanerz", "Minerale di titanio", "チタン鉱石", "钛矿石", "티타늄 광석"],
"item.uranium_ore": ["Uranium Ore", "Minério de urânio", "Minério de urânio", "Mineral de uranio", "Minerai d'uranium", "Uranerz", "Minerale di uranio", "ウラン鉱石", "铀矿石", "우라늄 광석"],
"item.crude_oil": ["Crude Oil (canister)", "Petróleo (bidão)", "Petróleo (galão)", "Petróleo (bidón)", "Pétrole (bidon)", "Rohöl (Kanister)", "Petrolio (tanica)", "原油（缶）", "原油（桶）", "원유 (통)"],
"item.alien_crystal": ["Alien Crystal", "Cristal alienígena", "Cristal alienígena", "Cristal alienígena", "Cristal extraterrestre", "Alien-Kristall", "Cristallo alieno", "異星の結晶", "外星水晶", "외계 수정"],
"item.iron_plate": ["Iron Plate", "Placa de ferro", "Placa de ferro", "Placa de hierro", "Plaque de fer", "Eisenplatte", "Piastra di ferro", "鉄板", "铁板", "철판"],
"item.copper_plate": ["Copper Plate", "Placa de cobre", "Placa de cobre", "Placa de cobre", "Plaque de cuivre", "Kupferplatte", "Piastra di rame", "銅板", "铜板", "구리판"],
"item.stone_brick": ["Stone Brick", "Tijolo de pedra", "Tijolo de pedra", "Ladrillo de piedra", "Brique de pierre", "Steinziegel", "Mattone di pietra", "石レンガ", "石砖", "돌 벽돌"],
"item.copper_wire": ["Copper Wire", "Fio de cobre", "Fio de cobre", "Cable de cobre", "Fil de cuivre", "Kupferdraht", "Filo di rame", "銅線", "铜线", "구리선"],
"item.alien_fruit": ["Alien Fruit", "Fruta alienígena", "Fruta alienígena", "Fruta alienígena", "Fruit extraterrestre", "Alien-Frucht", "Frutto alieno", "異星の果実", "外星果实", "외계 과일"],
"item.iron_gear": ["Iron Gear", "Engrenagem de ferro", "Engrenagem de ferro", "Engranaje de hierro", "Engrenage en fer", "Eisenzahnrad", "Ingranaggio di ferro", "鉄の歯車", "铁齿轮", "철 톱니바퀴"],
"item.circuit": ["Circuit", "Circuito", "Circuito", "Circuito", "Circuit", "Schaltkreis", "Circuito", "回路", "电路", "회로"],
"tech.logistics": ["Logistics", "Logística", "Logística", "Logística", "Logistique", "Logistik", "Logistica", "物流", "物流", "물류"],
"tech.agriculture": ["Agriculture", "Agricultura", "Agricultura", "Agricultura", "Agriculture", "Landwirtschaft", "Agricoltura", "農業", "农业", "농업"],
"tech.manufacturing": ["Manufacturing", "Fabricação", "Fabricação", "Fabricación", "Fabrication", "Fertigung", "Manifattura", "製造", "制造", "제조"],
"tech.advanced_computing": ["Advanced Computing", "Computação avançada", "Computação avançada", "Computación avanzada", "Informatique avancée", "Fortgeschrittene Datenverarbeitung", "Informatica avanzata", "高度計算", "高级计算", "고급 컴퓨팅"],
"tech.energy": ["Renewable Energy", "Energia renovável", "Energia renovável", "Energía renovable", "Énergie renouvelable", "Erneuerbare Energie", "Energia rinnovabile", "再生可能エネルギー", "可再生能源", "재생 에너지"],
"tech.cybersecurity": ["Cybersecurity", "Cibersegurança", "Cibersegurança", "Ciberseguridad", "Cybersécurité", "Cybersicherheit", "Sicurezza informatica", "サイバーセキュリティ", "网络安全", "사이버 보안"],
"tech.defense": ["Defense", "Defesa", "Defesa", "Defensa", "Défense", "Verteidigung", "Difesa", "防衛", "防御", "방어"],
"tech.ship_frame": ["Ship: Hull Frame", "Nave: estrutura do casco", "Nave: estrutura do casco", "Nave: armazón del casco", "Vaisseau : structure de coque", "Schiff: Rumpfgerüst", "Nave: telaio dello scafo", "宇宙船: 船体フレーム", "飞船：船体框架", "우주선: 선체 골격"],
"tech.ship_reactor": ["Ship: Reactor", "Nave: reator", "Nave: reator", "Nave: reactor", "Vaisseau : réacteur", "Schiff: Reaktor", "Nave: reattore", "宇宙船: 原子炉", "飞船：反应堆", "우주선: 원자로"],
"tech.ship_navigation": ["Ship: Navigation", "Nave: navegação", "Nave: navegação", "Nave: navegación", "Vaisseau : navigation", "Schiff: Navigation", "Nave: navigazione", "宇宙船: 航法", "飞船：导航", "우주선: 항법"],
"tech.ship_engines": ["Ship: Engines", "Nave: motores", "Nave: motores", "Nave: motores", "Vaisseau : moteurs", "Schiff: Triebwerke", "Nave: motori", "宇宙船: エンジン", "飞船：引擎", "우주선: 엔진"],
"tech.ship_launch": ["Ship: Launch Sequence", "Nave: sequência de lançamento", "Nave: sequência de lançamento", "Nave: secuencia de lanzamiento", "Vaisseau : séquence de lancement", "Schiff: Startsequenz", "Nave: sequenza di lancio", "宇宙船: 発射シーケンス", "飞船：发射程序", "우주선: 발사 절차"],
}

def main():
    here = os.path.dirname(os.path.abspath(__file__))
    more = {}
    exec(open(os.path.join(here, "tables_more.py"), encoding="utf-8").read(), more)   # -I: no imports from the script folder
    T.update(more["T2"])
    e5 = {}
    exec(open(os.path.join(here, "tables_etapa5.py"), encoding="utf-8").read(), e5)
    T.update(e5["T3"])
    e7 = {}
    exec(open(os.path.join(here, "tables_etapa7.py"), encoding="utf-8").read(), e7)
    T.update(e7["T4"])
    e8 = {}
    exec(open(os.path.join(here, "tables_etapa8.py"), encoding="utf-8").read(), e8)
    T.update(e8["T5"])
    fl = {}
    exec(open(os.path.join(here, "tables_fluids.py"), encoding="utf-8").read(), fl)
    T.update(fl["T6"])
    pw = {}
    exec(open(os.path.join(here, "tables_power.py"), encoding="utf-8").read(), pw)
    T.update(pw["T7"])
    sg = {}
    exec(open(os.path.join(here, "tables_signals.py"), encoding="utf-8").read(), sg)
    T.update(sg["T8"])
    rb = {}
    exec(open(os.path.join(here, "tables_robots.py"), encoding="utf-8").read(), rb)
    T.update(rb["T9"])
    out = os.path.join(here, "..", "..", "Assets", "StreamingAssets", "Localization")
    os.makedirs(out, exist_ok=True)
    for key, row in T.items():
        assert len(row) == len(LANGS), (key, len(row))
    for i, lang in enumerate(LANGS):
        table = {k: row[i] for k, row in T.items()}
        with open(os.path.join(out, lang + ".json"), "w", encoding="utf-8", newline="\n") as f:
            json.dump(table, f, ensure_ascii=False, indent=1, sort_keys=True)
    print(f"{len(T)} keys x {len(LANGS)} languages -> {os.path.normpath(out)}")

if __name__ == "__main__":
    main()
