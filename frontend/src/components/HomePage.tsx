import React, { useEffect, useState, FC } from 'react';
import ReactDOM from 'react-dom';
import { RouteComponentProps } from 'react-router-dom';

//Redux
import { connect } from 'react-redux';
import { ThunkDispatch } from 'redux-thunk';
import { AnyAction } from 'redux';
import { getUnansweredQuestionsActionCreator, AppState } from '../redux/Store';

/** @jsx jsx */
import { css, jsx } from '@emotion/core';
import { PrimaryButton } from '../style/Styles';

import { PageTitle } from '../style/PageTitle';

// Interfaces
import { QuestionsData } from '../interfaces/QuestionsData';

// Extra Components
import QuestionList from './QuestionList';
import Page from './Page';

//IF I WANT ONE PARAMETER CALLED WITH FUNCTION
const renderQuestion = (question: QuestionsData) => <div>{question.title}</div>;

interface Props extends RouteComponentProps {
  getUnansweredQuestions: () => Promise<void>;
  questions: QuestionsData[] | null;
  questionsLoading: boolean;
}

const HomePage: FC<Props> = ({
  history,
  questions,
  questionsLoading,
  getUnansweredQuestions,
}) => {
  // STATES
  const [count, setCount] = useState(0);

  useEffect(() => {
    if (questions === null) {
      getUnansweredQuestions();
    }
  }, [questions, getUnansweredQuestions]);

  // HANDLERS
  const handleAskQuestionClick = () => {
    history.push('./ask');
    // setCount(count + 1);
    // console.log('TODO - move to the AskPage');
  };

  return (
    <Page>
      <div
        css={css`
          display: flex;
          align-items: center;
          justify-content: space-between;
        `}
      >
        <PageTitle>Unanswered Questions</PageTitle>
        <PrimaryButton onClick={handleAskQuestionClick}>
          Ask a question
        </PrimaryButton>
      </div>

      {/* IF I WANT ONE PARAMETER CALLED WITH FUNCTION */}
      {/* <QuestionList QuestionsData={getUnansweredQuestions()} renderItem={renderQuestion}/> */}
      {questionsLoading ? (
        <div
          css={css`
            font-size: 16px;
            font-style: italic;
          `}
        >
          Loading...
        </div>
      ) : (
        <QuestionList
          QuestionsData={questions || []}
          // renderItem={renderQuestion}
        />
      )}
    </Page>
  );
};

const mapStateToProps = (store: AppState) => {
  return {
    questions: store.questions.unanswered,
    questionsLoading: store.questions.loading,
  };
};

const mapDispatchToProps = (dispatch: ThunkDispatch<any, any, AnyAction>) => {
  return {
    getUnansweredQuestions: () =>
      dispatch(getUnansweredQuestionsActionCreator()),
  };
};

export default connect(mapStateToProps, mapDispatchToProps)(HomePage);
