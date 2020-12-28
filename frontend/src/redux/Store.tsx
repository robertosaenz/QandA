import {
  QuestionsData,
  getUnansweredQuestions,
  postQuestion,
  PostQuestionData,
} from '../interfaces/QuestionsData';

import {
  Action,
  ActionCreator,
  Dispatch,
  Reducer,
  combineReducers,
  Store,
  createStore,
  applyMiddleware,
} from 'redux';

import thunk, { ThunkAction } from 'redux-thunk';

interface QuestionsState {
  readonly loading: boolean;
  readonly unanswered: QuestionsData[] | null;
  readonly postedResult?: QuestionsData;
}

export interface AppState {
  readonly questions: QuestionsState;
}

const initialQuestionState: QuestionsState = {
  loading: false,
  unanswered: null,
};

interface GettingUnansweredQuestionsAction
  extends Action<'GettingUnansweredQuestions'> {}

export interface GotUnansweredQuestionsAction
  extends Action<'GotUnansweredQuestions'> {
  questions: QuestionsData[];
}

export interface PostedQuestionAction extends Action<'PostedQuestion'> {
  result: QuestionsData | undefined;
}

type QuestionsActions =
  | GettingUnansweredQuestionsAction
  | GotUnansweredQuestionsAction
  | PostedQuestionAction;

export const getUnansweredQuestionsActionCreator: ActionCreator<
  ThunkAction<
    Promise<void>,
    QuestionsData[],
    null,
    GotUnansweredQuestionsAction
  >
> = () => {
  return async (dispatch: Dispatch) => {
    // TODO - dispatch the GettingUnansweredQuestions action
    // TODO - get the questions from server
    // TODO - dispatch the GotUnansweredQuestions action
    const gettingUnansweredQuestionsAction: GettingUnansweredQuestionsAction = {
      type: 'GettingUnansweredQuestions',
    };
    dispatch(gettingUnansweredQuestionsAction);
    const questions = await getUnansweredQuestions();

    const gotUnansweredQuestionAction: GotUnansweredQuestionsAction = {
      questions,
      type: 'GotUnansweredQuestions',
    };
    dispatch(gotUnansweredQuestionAction);
  };
};

export const postQuestionActionCreator: ActionCreator<
  ThunkAction<
    Promise<void>,
    QuestionsData,
    PostQuestionData,
    PostedQuestionAction
  >
> = (question: PostQuestionData) => {
  return async (dispatch: Dispatch) => {
    const result = await postQuestion(question);
    const postedQuestionAction: PostedQuestionAction = {
      type: 'PostedQuestion',
      result,
    };
    dispatch(postedQuestionAction);
  };
};

export const clearPostedQuestionActionCreator: ActionCreator<PostedQuestionAction> = () => {
  const postedQuestionAction: PostedQuestionAction = {
    type: 'PostedQuestion',
    result: undefined,
  };
  return postedQuestionAction;
};

const questionsReducer: Reducer<QuestionsState, QuestionsActions> = (
  state = initialQuestionState,
  action,
) => {
  // TODO - Handle the different actions and return new state
  switch (action.type) {
    case 'GettingUnansweredQuestions': {
      // TODO - return new state
      return {
        ...state,
        unanswered: null,
        loading: true,
      };
    }
    case 'GotUnansweredQuestions': {
      // TODO - return new state
      return {
        ...state,
        unanswered: action.questions,
        loading: false,
      };
    }
    case 'PostedQuestion': {
      // TODO - return new state
      return {
        ...state,
        unanswered: action.result
          ? (state.unanswered || []).concat(action.result)
          : state.unanswered,
        postedResult: action.result,
      };
    }
    default:
      neverReached(action);
  }

  return state;
};

const neverReached = (never: never) => {};

const rootReducer = combineReducers<AppState>({
  questions: questionsReducer,
});

export function configureStore(): Store<AppState> {
  const store = createStore(rootReducer, undefined, applyMiddleware(thunk));
  return store;
}
