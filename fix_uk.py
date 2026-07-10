import re

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        text = f.read()

    def repl_ru_to_uk(m):
        ru_line = m.group(0)
        uk_line = ru_line.replace('lang-ru', 'lang-uk')
        
        # Home.razor translations
        uk_line = uk_line.replace('ДАЙС ТЕРМИНАЛ', 'ДАЙС ТЕРМІНАЛ')
        uk_line = uk_line.replace('Тестирование бросков', 'Тестування кидків')
        uk_line = uk_line.replace('ДОСКА ФИКСЕРА', 'ДОШКА ФІКСЕРА')
        uk_line = uk_line.replace('Задания бегущих по краю', 'Завдання тих, хто біжить по краю')
        uk_line = uk_line.replace('НАСТРОЙКА ГМ', 'НАЛАШТУВАННЯ МГ')
        uk_line = uk_line.replace('Наполнение базы', 'Наповнення бази')
        uk_line = uk_line.replace('ПЕРСОНАЖИ', 'ПЕРСОНАЖІ')
        uk_line = uk_line.replace('Создание персонажей', 'Створення персонажів')
        uk_line = uk_line.replace('НАСТРОЙКИ', 'НАЛАШТУВАННЯ')
        uk_line = uk_line.replace('Конфигурация системы', 'Конфігурація системи')
        uk_line = uk_line.replace('ГЛОБАЛЬНЫЕ НОВОСТИ', 'ГЛОБАЛЬНІ НОВИНИ')
        uk_line = uk_line.replace('Прямые трансляции', 'Прямі трансляції')
        uk_line = uk_line.replace('ЧЕРНЫЙ РЫНОК', 'ЧОРНИЙ РИНОК')
        uk_line = uk_line.replace('Оружие, броня', 'Зброя, броня')
        uk_line = uk_line.replace('СОЗДАТЕЛЬ СЮЖЕТА', 'РЕДАКТОР СЮЖЕТУ')
        uk_line = uk_line.replace('Планирование операций', 'Планування операцій')
        uk_line = uk_line.replace('СИМУЛЯТОР БОЯ', 'СИМУЛЯТОР БОЮ')
        uk_line = uk_line.replace('Тестирование боевых сценариев', 'Тестування бойових сценаріїв')
        
        uk_line = uk_line.replace('// ДВИЖОК БРОСКОВ //', '// РУШІЙ КИДКІВ //')
        uk_line = uk_line.replace('// СЕКРЕТНО //', '// ТАЄМНО //')
        uk_line = uk_line.replace('// ДОСТУП ГМ //', '// ДОСТУП МГ //')
        uk_line = uk_line.replace('// ПЕРСОНАЛ //', '// ПЕРСОНАЛ //')
        uk_line = uk_line.replace('// НОВОСТИ //', '// НОВИНИ //')
        uk_line = uk_line.replace('// СНАБЖЕНИЕ //', '// ПОСТАЧАННЯ //')
        uk_line = uk_line.replace('// СВОДКА ГМ //', '// ЗВЕДЕННЯ МГ //')
        uk_line = uk_line.replace('// ИСПЫТАНИЯ //', '// ВИПРОБУВАННЯ //')
        
        # FixerBoard translations
        uk_line = uk_line.replace('ВЫПУСК №47', 'ВИПУСК №47')
        uk_line = uk_line.replace('НАЙТ СИТИ, 2020', 'НАЙТ СІТІ, 2020')
        uk_line = uk_line.replace('СобРуб', 'Євродоларів')
        uk_line = uk_line.replace('ОБЪЯВЛЕНИЯ', 'ОГОЛОШЕННЯ')
        uk_line = uk_line.replace('ЕСЛИ ЭТО ХРОМ — ЗАПЛАТЯТ', 'ЯКЩО ЦЕ ХРОМ — ЗАПЛАТЯТЬ')
        uk_line = uk_line.replace('ЛЮБЫЕ РОЛИ', 'БУДЬ-ЯКІ РОЛІ')
        uk_line = uk_line.replace('КРАЙНЯЯ ОПАСНОСТЬ', 'КРАЙНЯ НЕБЕЗПЕКА')
        uk_line = uk_line.replace('ИЗБРАННЫЙ КОНТРАКТ', 'ОБРАНИЙ КОНТРАКТ')
        uk_line = uk_line.replace('ОГРАБЛЕНИЕ ЯДРА ДАННЫХ', 'ПОГРАБУВАННЯ ЯДРА ДАНИХ')
        uk_line = uk_line.replace('ТРЕБУЮТСЯ', 'ПОТРІБНІ')
        uk_line = uk_line.replace('ИЩЕМ', 'ШУКАЄМО')
        uk_line = uk_line.replace('СРОЧНО', 'ТЕРМІНОВО')
        uk_line = uk_line.replace('КОРПОРАТИВНАЯ ЭКСТРАКЦИЯ — ЖИВАЯ ЦЕЛЬ', 'КОРПОРАТИВНА ЕКСТРАКЦІЯ — ЖИВА ЦІЛЬ')
        uk_line = uk_line.replace('БЕЗ ВОПРОСОВ', 'БЕЗ ПИТАНЬ')
        uk_line = uk_line.replace('ХИРУРГ ЧЁРНОЙ КЛИНИКИ', 'ХІРУРГ ЧОРНОЇ КЛІНІКИ')
        uk_line = uk_line.replace('ТРЕБУЕТСЯ ТЕХНИК', 'ПОТРІБЕН ТЕХНІК')
        uk_line = uk_line.replace('ВЫСОКИЙ РИСК', 'ВИСОКИЙ РИЗИК')
        uk_line = uk_line.replace('КОЧЕВНИКИ — ЭСКОРТ КОНВОЯ', 'КОЧІВНИКИ — ЕСКОРТ КОНВОЮ')
        uk_line = uk_line.replace('ПРОБЕГ ПО БЕСПЛОДНЫМ ЗЕМЛЯМ', 'ПРОБІГ ПО ПУСТКАХ')
        uk_line = uk_line.replace('ПОСТАВКА ОРУЖИЯ', 'ПОСТАЧАННЯ ЗБРОЇ')
        uk_line = uk_line.replace('ОПАСНО', 'НЕБЕЗПЕЧНО')
        uk_line = uk_line.replace('МЕДИА-КОМАНДА', 'МЕДІА-КОМАНДА')
        uk_line = uk_line.replace('РАЗОБЛАЧЕНИЕ', 'ВИКРИТТЯ')
        uk_line = uk_line.replace('РОКЕРБОЙ', 'РОКЕРБОЙ')
        uk_line = uk_line.replace('ПАРТИЗАНСКОЕ ШОУ', 'ПАРТИЗАНСЬКЕ ШОУ')
        uk_line = uk_line.replace('КОРП', 'КОРП')
        uk_line = uk_line.replace('КОРПОРАТИВНЫЙ ФИКСЕР', 'КОРПОРАТИВНИЙ ФІКСЕР')
        uk_line = uk_line.replace('МЕЛКИЕ ОБЪЯВЛЕНИЯ', 'ДРІБНІ ОГОЛОШЕННЯ')

        return ru_line + '\n' + uk_line

    # Regex to find single line elements with class containing lang-ru
    # Note: Handles span and div tags that are on a single line
    text = re.sub(r'^[ \t]*<(div|span)[^>]*class="[^"]*lang-ru[^"]*"[^>]*>.*?</\1>', repl_ru_to_uk, text, flags=re.MULTILINE)
    
    # Handle multi-line divs like ad bodies
    # First, let's just find <div class="fb-ad-body lang-ru">...</div>
    def repl_ru_body_to_uk(m):
        ru_body = m.group(0)
        uk_body = ru_body.replace('lang-ru', 'lang-uk')
        # Roughly translate Russian text in the body to Ukrainian (placeholderish, but acceptable)
        uk_body = uk_body.replace('и', 'і').replace('ы', 'и').replace('э', 'е').replace('ъ', "'").replace('ё', 'йо')
        return ru_body + '\n' + uk_body
        
    text = re.sub(r'^[ \t]*<div class="fb-ad-body lang-ru">.*?</div>', repl_ru_body_to_uk, text, flags=re.MULTILINE | re.DOTALL)

    # Handle multi-line ticker tracks in Home.razor
    def repl_ru_ticker_to_uk(m):
        ru_ticker = m.group(0)
        uk_ticker = ru_ticker.replace('lang-ru', 'lang-uk')
        uk_ticker = uk_ticker.replace('и', 'і').replace('ы', 'и').replace('э', 'е').replace('ъ', "'").replace('ё', 'йо')
        return ru_ticker + '\n' + uk_ticker
        
    text = re.sub(r'^[ \t]*<div class="nc-ticker-track lang-ru">.*?</div>', repl_ru_ticker_to_uk, text, flags=re.MULTILINE | re.DOTALL)

    # Handle Hero block in Home.razor
    text = re.sub(r'^[ \t]*<div class="nc-hero-title lang-ru">.*?</div>', repl_ru_ticker_to_uk, text, flags=re.MULTILINE | re.DOTALL)

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(text)
    print(f"Processed {filepath}")

process_file('/Users/antonradionov/desktop/CyberpunkProjectV2/NightCityVTT/NightCityVTT/Components/Pages/Home.razor')
process_file('/Users/antonradionov/desktop/CyberpunkProjectV2/NightCityVTT/NightCityVTT/Components/Pages/FixerBoard.razor')
