import React, { FC, useState, Fragment, useEffect } from 'react';
import { RouteComponentProps } from 'react-router-dom';

// Signal R
import {
  HubConnectionBuilder,
  HubConnectionState,
  HubConnection,
} from '@aspnet/signalr';

/** @jsx jsx */
import { css, jsx } from '@emotion/core';
import { gray3, gray6 } from '../style/Styles';

// Interfaces
import {
  QuestionsData,
  getQuestion,
  postAnswer,
  mapQuestionFromServer,
  QuestionDataFromServer,
} from '../interfaces/QuestionsData';

// Extra Components
import Page from './Page';
import AnswerList from './AnswerList';
import { Form, required, minLength, Values_Interface } from './Form';
import { Field } from './Field';

interface RouteProps {
  questionId: string;
}

const QuestionPage: FC<RouteComponentProps<RouteProps>> = ({ match }) => {
  const [question, setQuestion] = useState<QuestionsData | null>(null);

  // Signal R
  const setUpSignalRConnection = async (questionId: number) => {
    // TODO - setup connection to real-time SignalR API
    const connection = new HubConnectionBuilder()
      .withUrl('https://localhost:44311/questionshub')
      .withAutomaticReconnect()
      .build();

    // TODO - handle Message function being called
    connection.on('Message', (message: string) => {
      console.log('Message', message);
    });

    // TODO - handle ReceiveQuestion function being called
    connection.on('ReceiveQuestion', (question: QuestionDataFromServer) => {
      console.log('ReceiveQuestion', question);
      setQuestion(mapQuestionFromServer(question));
    });

    // TODO - start the connection
    try {
      await connection.start();
    } catch (err) {
      console.log(err);
    }

    // TODO - subscribe to question
    if (connection.state === HubConnectionState.Connected) {
      connection.invoke('SubscribeQuestion', questionId).catch((err: Error) => {
        return console.error(err.toString());
      });
    }

    return connection;
  };

  const cleanUpSignalRConnection = async (
    questionId: number,
    connection: HubConnection,
  ) => {
    // TODO - unsubscribe from the question
    if (connection.state === HubConnectionState.Connected) {
      try {
        await connection.invoke('UnsubscribeQuestion', questionId);
      } catch (err) {
        return console.error(err.toString());
      }
      connection.off('Message');
      connection.off('ReceiveQuestion');
      connection.stop();
    } else {
      connection.off('Message');
      connection.off('ReceiveQuestion');
      connection.stop();
    }
    // TODO - stop the connection
  };

  useEffect(() => {
    const doGetQuestion = async (questionId: number) => {
      const foundQuestion = await getQuestion(questionId);
      setQuestion(foundQuestion);
    };
    // Signal R
    let connection: HubConnection;

    if (match.params.questionId) {
      const questionId = Number(match.params.questionId);
      doGetQuestion(questionId);
      // Signal R
      setUpSignalRConnection(questionId).then((con) => {
        connection = con;
      });
    }

    // Signal R
    return function cleanUp() {
      if (match.params.questionId) {
        const questionId = Number(match.params.questionId);
        cleanUpSignalRConnection(questionId, connection);
      }
    };
  }, [match.params.questionId]);

  const handleSubmit = async (values: Values_Interface) => {
    const result = await postAnswer({
      questionId: question!.questionId,
      content: values.content,
      userName: 'Fred',
      created: new Date(),
    });
    return { success: result ? true : false };
  };

  return (
    <Page>
      <div
        css={css`
          background-color: white;
          padding: 15px 20px 20px 20px;
          border-radius: 4px;
          border: 1px solid ${gray6};
          box-shadow: 0 3px 5px 0 rgba(0, 0, 0, 0.16);
        `}
      >
        <div
          css={css`
            font-size: 19px;
            font-weight: bold;
            margin: 10px 0px 5px;
          `}
        >
          {question === null ? '' : question.title}
        </div>
        {question !== null && (
          <Fragment>
            <p
              css={css`
                margin-top: 0px;
                background-color: white;
              `}
            >
              {question.content}
            </p>
            <AnswerList AnswersData={question.answers} />
            <div
              css={css`
                margin-top: 20px;
              `}
            >
              <Form
                submitCaption="Submit Your Answer"
                validationRules={{
                  content: [
                    { validator: required },
                    { validator: minLength, arg: 50 },
                  ],
                }}
                onSubmit={handleSubmit}
                failureMessage="There was a problem with your answer"
                successMessage="Your answer was successfully submitted"
              >
                <Field name="content" label="Your Answer" type="TextArea" />
              </Form>
            </div>
          </Fragment>
        )}
      </div>
    </Page>
  );
};

export default QuestionPage;
