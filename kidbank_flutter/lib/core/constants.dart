class ApiConstants {
  static const String baseUrl = 'http://10.0.2.2:5000/api/v1';
  static const Duration timeout = Duration(seconds: 30);
}

class AppStrings {
  static const appName = 'KidBank';
  static const currency = 'BYN';

  // Auth
  static const login = 'Войти';
  static const register = 'Регистрация';
  static const email = 'Email';
  static const password = 'Пароль';
  static const firstName = 'Имя';
  static const lastName = 'Фамилия';
  static const dateOfBirth = 'Дата рождения';
  static const familyName = 'Название семьи';
  static const invitationCode = 'Код приглашения';
  static const registerAsParent = 'Зарегистрироваться как родитель';
  static const registerAsKid = 'Зарегистрироваться как ребёнок';
  static const iHaveAccount = 'Уже есть аккаунт? Войти';
  static const noAccount = 'Нет аккаунта? Зарегистрироваться';
  static const agreementText = 'Я принимаю условия';
  static const offerAgreement = 'публичной оферты';
  static const privacyPolicy = 'политики конфиденциальности';
  static const parentConsent = 'Я даю согласие на создание банковского счёта для моего ребёнка в соответствии с законодательством РБ';
  static const logout = 'Выйти';

  // Parent
  static const familyDashboard = 'Семья';
  static const myKids = 'Мои дети';
  static const addKid = 'Пригласить ребёнка';
  static const tasks = 'Задания';
  static const taskTemplates = 'Шаблоны заданий';
  static const reports = 'Отчёты';
  static const analytics = 'Аналитика';
  static const spendingLimits = 'Лимиты расходов';
  static const moneyRequests = 'Запросы денег';
  static const allowance = 'Карманные деньги';

  // Kid
  static const myAccounts = 'Мои счета';
  static const myGoals = 'Мои цели';
  static const myTasks = 'Мои задания';
  static const myCards = 'Мои карты';
  static const requestMoney = 'Запросить деньги';
  static const education = 'Обучение';
  static const achievements = 'Достижения';
  static const leaderboard = 'Рейтинг';

  // Common
  static const chat = 'Чат';
  static const notifications = 'Уведомления';
  static const settings = 'Настройки';
  static const profile = 'Профиль';
  static const save = 'Сохранить';
  static const cancel = 'Отмена';
  static const delete = 'Удалить';
  static const confirm = 'Подтвердить';
  static const loading = 'Загрузка...';
  static const error = 'Ошибка';
  static const success = 'Успешно';
  static const noData = 'Данные отсутствуют';
}
